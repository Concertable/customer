[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $OutputPath,

    [switch] $KeepArtifacts
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$repositoryUrl = 'https://github.com/Concertable/customer'
$revision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($revision)) {
    throw 'Could not resolve the Customer source revision.'
}

$workingTreeChanges = @(& git -C $repositoryRoot status --porcelain --untracked-files=all)
if ($LASTEXITCODE -ne 0) {
    throw 'Could not inspect the Customer working tree.'
}
if ($workingTreeChanges.Count -gt 0) {
    throw 'Refusing to prepare a Customer release candidate from a dirty or untracked working tree.'
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    if ($KeepArtifacts) {
        throw 'OutputPath is required when KeepArtifacts is specified.'
    }

    $releaseRoot = Join-Path ([System.IO.Path]::GetTempPath()) "concertable-customer-release-candidate-$([Guid]::NewGuid().ToString('N'))"
}
elseif ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $releaseRoot = [System.IO.Path]::GetFullPath($OutputPath)
}
else {
    $releaseRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputPath))
}

$repositoryPrefix = $repositoryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if ($releaseRoot.Equals($repositoryRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
    $releaseRoot.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Release-candidate output must be outside the Customer repository so the image build context remains clean.'
}

if (Test-Path -LiteralPath $releaseRoot) {
    throw "Release-candidate output already exists: '$releaseRoot'."
}

# The promotion manifest is the repository's own declaration of what it publishes; deriving the
# expected set from it means a package added there and nowhere else cannot slip through unverified,
# and a set that drifts fails here rather than at the registry.
$promotionManifest = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot '.github/customer-promotion-candidates.json') | ConvertFrom-Json
$expectedPackageIds = @($promotionManifest.nuget | ForEach-Object { $_.id } | Sort-Object)
$packageProjects = @($promotionManifest.nuget | ForEach-Object { Join-Path $repositoryRoot $_.project })
$expectedImageRepositories = @($promotionManifest.oci | ForEach-Object { $_.repository } | Sort-Object)

$markerPath = Join-Path $releaseRoot '.customer-release-candidate'
$packageRoot = Join-Path $releaseRoot 'packages'
$imageRoot = Join-Path $releaseRoot 'images'
$manifestPath = Join-Path $releaseRoot 'release-manifest.json'
$evidenceRoot = Join-Path $releaseRoot 'evidence'
$integrityRoot = Join-Path $repositoryRoot 'artifacts/release-candidate'
$packageToken = $env:GITHUB_PACKAGES_TOKEN
$releaseRootCreated = $false
$integrityStaged = $false
$completed = $false
$builtImages = @()

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Value
    )

    [System.IO.File]::WriteAllText($Path, $Value, [System.Text.UTF8Encoding]::new($false))
}

function Get-NuGetIdentity {
    param([Parameter(Mandatory)][System.IO.FileInfo] $Package)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Package.FullName)
    try {
        $manifestEntry = $archive.Entries | Where-Object FullName -Like '*.nuspec' | Select-Object -First 1
        if ($null -eq $manifestEntry) {
            throw "Package '$($Package.Name)' does not contain a NuGet manifest."
        }

        $reader = [System.IO.StreamReader]::new($manifestEntry.Open())
        try {
            [xml] $manifest = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $metadata = $manifest.package.metadata
        if ([string] $metadata.repository.url -ne $repositoryUrl -or [string] $metadata.projectUrl -ne $repositoryUrl) {
            throw "Package '$($Package.Name)' does not identify the canonical Customer repository."
        }
        if ([string] $metadata.repository.commit -ne $revision) {
            throw "Package '$($Package.Name)' does not identify exact Customer revision '$revision'."
        }
        if ([string] $metadata.readme -ne 'README.md') {
            throw "Package '$($Package.Name)' does not declare its package README."
        }
        if ([string]::IsNullOrWhiteSpace([string] $metadata.description) -or [string] $metadata.description -eq 'Package Description') {
            throw "Package '$($Package.Name)' does not have a meaningful description."
        }

        return [ordered]@{ Id = [string] $metadata.id; Version = [string] $metadata.version }
    }
    finally {
        $archive.Dispose()
    }
}

function Get-ArtifactRecord {
    param([Parameter(Mandatory)][string] $Path)

    $item = Get-Item -LiteralPath $Path
    $releasePrefix = $releaseRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $item.FullName.StartsWith($releasePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Artifact '$($item.FullName)' is outside the release-candidate root."
    }

    $stream = [System.IO.File]::OpenRead($item.FullName)
    try {
        $hasher = [System.Security.Cryptography.SHA256]::Create()
        try {
            $sha256 = ([System.BitConverter]::ToString($hasher.ComputeHash($stream))).Replace('-', '').ToLowerInvariant()
        }
        finally {
            $hasher.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }

    return [ordered]@{
        path = $item.FullName.Substring($releasePrefix.Length).Replace('\', '/')
        sha256 = $sha256
        bytes = $item.Length
    }
}

try {
    if ([string]::IsNullOrWhiteSpace($packageToken)) {
        throw 'GITHUB_PACKAGES_TOKEN is required for Customer release-candidate verification.'
    }

    New-Item -ItemType Directory -Path $releaseRoot | Out-Null
    $releaseRootCreated = $true
    Write-Utf8NoBom -Path $markerPath -Value 'Concertable.Customer release candidate'
    New-Item -ItemType Directory -Path $packageRoot, $imageRoot | Out-Null

    foreach ($project in $packageProjects) {
        & dotnet restore $project --force-evaluate
        if ($LASTEXITCODE -ne 0) {
            throw "Customer package restore failed for '$project' with exit code $LASTEXITCODE."
        }

        & dotnet build $project --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "Customer package build failed for '$project' with exit code $LASTEXITCODE."
        }

        & dotnet pack $project --configuration $Configuration --no-build --no-restore --output $packageRoot
        if ($LASTEXITCODE -ne 0) {
            throw "Customer package creation failed for '$project' with exit code $LASTEXITCODE."
        }
    }

    # Composes the repository's existing gate rather than restating its checks: it builds a clean
    # consumer that actually constructs types from every published id.
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'verify-package-candidates.ps1') -PackageDirectory $packageRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Customer package candidate verification failed with exit code $LASTEXITCODE."
    }

    $packages = @(Get-ChildItem -LiteralPath $packageRoot -Filter '*.nupkg' -File |
        Where-Object Name -NotLike '*.symbols.nupkg')
    $packageRecords = @($packages | ForEach-Object {
        $identity = Get-NuGetIdentity -Package $_
        [ordered]@{
            id = $identity.Id
            version = $identity.Version
            artifact = Get-ArtifactRecord -Path $_.FullName
        }
    } | Sort-Object { $_.id })

    $actualPackageIds = @($packageRecords | ForEach-Object { $_.id })
    if (($actualPackageIds -join ',') -ne ($expectedPackageIds -join ',')) {
        throw "Unexpected Customer release-candidate package set '$($actualPackageIds -join ',')'."
    }

    $versions = @($packageRecords | ForEach-Object { $_.version } | Sort-Object -Unique)
    if ($versions.Count -ne 1 -or $versions[0] -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') {
        throw "Customer packages do not share one valid SemVer version: '$($versions -join ',')'."
    }
    $releaseVersion = $versions[0]

    # The monorepo published Customer's retained ids into 0.1.x and MinVer counts height from this
    # repository alone, so a floor regression would publish beneath the feed rather than above it.
    if ([version] ($releaseVersion.Split('-', 2)[0]) -lt [version] '0.2.0') {
        throw "Customer release-candidate version '$releaseVersion' is below the independent 0.2.0 baseline that clears the retained 0.1.x train."
    }

    $imageResultPath = Join-Path ([System.IO.Path]::GetTempPath()) "customer-images-$([Guid]::NewGuid().ToString('N')).json"
    try {
        & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'verify-customer-images.ps1') `
            -Configuration $Configuration `
            -ArchiveDirectory $imageRoot `
            -BuildVersion $releaseVersion `
            -ResultPath $imageResultPath `
            -KeepImages
        if ($LASTEXITCODE -ne 0) {
            throw "Customer image verification failed with exit code $LASTEXITCODE."
        }

        $imageResults = @(Get-Content -Raw -LiteralPath $imageResultPath | ConvertFrom-Json)
    }
    finally {
        Remove-Item -LiteralPath $imageResultPath -Force -ErrorAction SilentlyContinue
    }
    $builtImages = @($imageResults | ForEach-Object { $_.Image })

    $actualImageRepositories = @($imageResults | ForEach-Object { $_.Repository } | Sort-Object)
    if (($actualImageRepositories -join ',') -ne ($expectedImageRepositories -join ',')) {
        throw "Unexpected Customer release-candidate image set '$($actualImageRepositories -join ',')'."
    }

    $imageRecords = @($imageResults | ForEach-Object {
        [ordered]@{
            repository = $_.Repository
            version = $releaseVersion
            sourceRevision = $revision
            intendedTags = @($releaseVersion, $revision)
            localImageId = $_.LocalImageId
            archive = Get-ArtifactRecord -Path $_.Archive
        }
    } | Sort-Object { $_.repository })

    # verify-artifact-integrity.ps1 resolves its evidence directory against the repository root and
    # mounts that root into the scanner containers, so candidates must be staged inside the repository.
    # Staging after the images are built keeps the image build context clean.
    $integrityPackages = Join-Path $integrityRoot 'packages'
    $integrityImages = Join-Path $integrityRoot 'images'
    if (Test-Path -LiteralPath $integrityRoot) {
        Remove-Item -LiteralPath $integrityRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Path $integrityPackages, $integrityImages | Out-Null
    $integrityStaged = $true
    Copy-Item -LiteralPath @($packages.FullName) -Destination $integrityPackages
    Copy-Item -LiteralPath @($imageResults.Archive) -Destination $integrityImages

    # Absolute for the candidate directories, which resolve against the caller's working directory;
    # repository-relative for the evidence directory, which is joined onto the repository root.
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'verify-artifact-integrity.ps1') `
        -PackageDirectory $integrityPackages `
        -ImageDirectory $integrityImages `
        -EvidenceDirectory 'artifacts/release-candidate/evidence'
    if ($LASTEXITCODE -ne 0) {
        throw "Customer artifact integrity verification failed with exit code $LASTEXITCODE."
    }

    Copy-Item -LiteralPath (Join-Path $integrityRoot 'evidence') -Destination $evidenceRoot -Recurse
    $evidenceRecords = @(Get-ChildItem -LiteralPath $evidenceRoot -Recurse -File |
        Sort-Object FullName |
        ForEach-Object { Get-ArtifactRecord -Path $_.FullName })
    if ($evidenceRecords.Count -eq 0) {
        throw 'Customer release candidate carries no scan evidence.'
    }

    $manifest = [ordered]@{
        schemaVersion = 1
        repository = $repositoryUrl
        sourceRevision = $revision
        version = $releaseVersion
        packages = $packageRecords
        images = $imageRecords
        evidence = $evidenceRecords
    }
    Write-Utf8NoBom -Path $manifestPath -Value ($manifest | ConvertTo-Json -Depth 12)

    $verifiedManifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
    if ($verifiedManifest.sourceRevision -ne $revision -or
        $verifiedManifest.version -ne $releaseVersion -or
        @($verifiedManifest.packages).Count -ne $expectedPackageIds.Count -or
        @($verifiedManifest.images).Count -ne $expectedImageRepositories.Count) {
        throw 'Customer release-candidate manifest validation failed.'
    }

    $expectedArtifactPaths = @($packageRecords.artifact.path; $imageRecords.archive.path; $evidenceRecords.path) | Sort-Object
    $releasePrefix = $releaseRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $actualArtifactPaths = @(Get-ChildItem -LiteralPath $releaseRoot -Force -Recurse -File |
        Where-Object FullName -NotIn @($markerPath, $manifestPath) |
        ForEach-Object { $_.FullName.Substring($releasePrefix.Length).Replace('\', '/') } |
        Sort-Object)
    if (($actualArtifactPaths -join "`n") -ne ($expectedArtifactPaths -join "`n")) {
        throw "Release-candidate bundle contains unmanifested files: '$($actualArtifactPaths -join ', ')'."
    }

    $completed = $true
    Write-Host "Verified Customer release candidate $releaseVersion for revision ${revision}: $($expectedPackageIds.Count) packages, $($expectedImageRepositories.Count) images, $($evidenceRecords.Count) evidence files, manifest complete."
    if ($KeepArtifacts) {
        Write-Host "Retained release-candidate artifacts at '$releaseRoot'."
    }
}
finally {
    foreach ($image in $builtImages) {
        & docker image rm --force $image 2>$null | Out-Null
    }

    # A failed run's evidence is the only record of why it failed, and the integrity script writes its
    # failure there, so the staging directory survives anything but success.
    if ($integrityStaged -and (Test-Path -LiteralPath $integrityRoot)) {
        if ($completed) {
            Remove-Item -LiteralPath $integrityRoot -Recurse -Force
        }
        else {
            Write-Host "Retained integrity evidence at '$integrityRoot'."
        }
    }

    if ($releaseRootCreated -and (-not $KeepArtifacts -or -not $completed)) {
        if (Test-Path -LiteralPath $markerPath -PathType Leaf) {
            Remove-Item -LiteralPath $releaseRoot -Recurse -Force
        }
        else {
            throw "Refusing to remove release-candidate root '$releaseRoot' without its marker."
        }
    }
}
