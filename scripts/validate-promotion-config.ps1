[CmdletBinding()]
param(
    [string] $ManifestPath = '.github/customer-promotion-candidates.json',

    [string] $PackageDirectory,

    [string] $ImageDirectory,

    [string] $ReleaseTag,

    [string] $ExpectedCommit
)

$ErrorActionPreference = 'Stop'

function Assert-ExactSet {
    param(
        [Parameter(Mandatory)]
        [string[]] $Expected,

        [Parameter(Mandatory)]
        [string[]] $Actual,

        [Parameter(Mandatory)]
        [string] $Description
    )

    $difference = Compare-Object -ReferenceObject @($Expected | Sort-Object) `
                                 -DifferenceObject @($Actual | Sort-Object) `
                                 -CaseSensitive
    if ($difference) {
        throw "$Description do not match the promotion manifest. Expected: $($Expected -join ', '). Actual: $($Actual -join ', ')."
    }
}

function Resolve-RepositoryPath {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [StringComparison] $Comparison
    )

    $resolved = [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Path))
    if (-not $resolved.StartsWith("$RepositoryRoot$([System.IO.Path]::DirectorySeparatorChar)", $Comparison)) {
        throw "Path '$Path' escapes the repository root."
    }

    return $resolved
}

function Read-PackageMetadata {
    param([Parameter(Mandatory)] [System.IO.FileInfo] $Package)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Package.FullName)
    try {
        $nuspec = @($archive.Entries | Where-Object {
            $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase)
        })
        if ($nuspec.Count -ne 1) {
            throw "Package '$($Package.Name)' contains $($nuspec.Count) nuspec files."
        }

        $stream = $nuspec[0].Open()
        try {
            $document = [System.Xml.XmlDocument]::new()
            $document.Load($stream)
            $metadata = $document.DocumentElement.ChildNodes |
                Where-Object { $_.LocalName -eq 'metadata' } |
                Select-Object -First 1
            $id = $metadata.ChildNodes | Where-Object { $_.LocalName -eq 'id' } | Select-Object -First 1
            $version = $metadata.ChildNodes | Where-Object { $_.LocalName -eq 'version' } | Select-Object -First 1
            if ($null -eq $id -or $null -eq $version) {
                throw "Package '$($Package.Name)' has incomplete nuspec metadata."
            }

            return [pscustomobject]@{ Id = $id.InnerText; Version = $version.InnerText }
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$pathComparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
$resolvedManifestPath = Resolve-RepositoryPath -RepositoryRoot $repositoryRoot -Path $ManifestPath -Comparison $pathComparison
if (-not [System.IO.File]::Exists($resolvedManifestPath)) {
    throw "Promotion manifest '$ManifestPath' does not exist."
}

$manifest = Get-Content -LiteralPath $resolvedManifestPath -Raw | ConvertFrom-Json -Depth 10
if ($manifest.schemaVersion -ne 1) {
    throw "Unsupported promotion manifest schema version '$($manifest.schemaVersion)'."
}

$nuget = @($manifest.nuget)
$oci = @($manifest.oci)
if ($nuget.Count -ne 4 -or $oci.Count -ne 3) {
    throw "The Customer promotion manifest must select exactly four NuGet and three OCI candidates."
}

$nugetIds = @($nuget.id)
$ociRepositories = @($oci.repository)
$ociArchives = @($oci.archive)
if (@($nugetIds | Sort-Object -Unique -CaseSensitive).Count -ne $nuget.Count) {
    throw 'The promotion manifest contains duplicate NuGet IDs.'
}
if (@($ociRepositories | Sort-Object -Unique -CaseSensitive).Count -ne $oci.Count) {
    throw 'The promotion manifest contains duplicate OCI repositories.'
}
if (@($ociArchives | Sort-Object -Unique -CaseSensitive).Count -ne $oci.Count) {
    throw 'The promotion manifest contains duplicate OCI archive names.'
}

foreach ($candidate in $nuget) {
    $projectPath = Resolve-RepositoryPath -RepositoryRoot $repositoryRoot -Path $candidate.project -Comparison $pathComparison
    if (-not [System.IO.File]::Exists($projectPath)) {
        throw "NuGet candidate project '$($candidate.project)' does not exist."
    }

    [xml] $project = Get-Content -LiteralPath $projectPath -Raw
    $isPackable = @($project.Project.PropertyGroup.IsPackable | Where-Object { $_ -ne $null })
    if ($isPackable.Count -eq 0 -or -not ($isPackable -contains 'true')) {
        throw "NuGet candidate '$($candidate.id)' is not explicitly packable."
    }

    $declaredId = @($project.Project.PropertyGroup.PackageId | Where-Object { $_ -ne $null }) | Select-Object -Last 1
    if ([string]::IsNullOrWhiteSpace($declaredId)) {
        $declaredId = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    }
    if ($declaredId -cne $candidate.id) {
        throw "NuGet candidate '$($candidate.project)' declares '$declaredId', not '$($candidate.id)'."
    }
}

foreach ($candidate in $oci) {
    $projectPath = Resolve-RepositoryPath -RepositoryRoot $repositoryRoot -Path $candidate.project -Comparison $pathComparison
    if (-not [System.IO.File]::Exists($projectPath)) {
        throw "OCI candidate project '$($candidate.project)' does not exist."
    }

    [xml] $project = Get-Content -LiteralPath $projectPath -Raw
    $containerRepository = @($project.Project.PropertyGroup.ContainerRepository | Where-Object { $_ -ne $null }) |
        Select-Object -Last 1
    $expectedRepository = "ghcr.io/$containerRepository"
    if ($expectedRepository -cne $candidate.repository) {
        throw "OCI candidate '$($candidate.project)' declares '$expectedRepository', not '$($candidate.repository)'."
    }
}

if ([string]::IsNullOrWhiteSpace($PackageDirectory) -xor [string]::IsNullOrWhiteSpace($ImageDirectory)) {
    throw 'PackageDirectory and ImageDirectory must be supplied together.'
}

$packageMetadata = @()
if (-not [string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $resolvedPackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
    $resolvedImageDirectory = (Resolve-Path -LiteralPath $ImageDirectory).Path
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packages = @(Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter '*.nupkg' -File |
        Where-Object { -not $_.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase) })
    $images = @(Get-ChildItem -LiteralPath $resolvedImageDirectory -Filter '*.tar.gz' -File)
    $packageMetadata = @($packages | ForEach-Object { Read-PackageMetadata -Package $_ })

    Assert-ExactSet -Expected $nugetIds -Actual @($packageMetadata.Id) -Description 'NuGet candidate IDs'
    Assert-ExactSet -Expected $ociArchives -Actual @($images.Name) -Description 'OCI candidate archives'
}

if ([string]::IsNullOrWhiteSpace($ReleaseTag) -xor [string]::IsNullOrWhiteSpace($ExpectedCommit)) {
    throw 'ReleaseTag and ExpectedCommit must be supplied together.'
}

if (-not [string]::IsNullOrWhiteSpace($ReleaseTag)) {
    if ($ReleaseTag -notmatch '^v(?<version>[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?)$') {
        throw "ReleaseTag '$ReleaseTag' is not a v-prefixed semantic version."
    }
    $releaseVersion = $Matches.version
    if ($ExpectedCommit -notmatch '^[0-9a-f]{40}$') {
        throw "ExpectedCommit '$ExpectedCommit' is not a full lowercase Git SHA."
    }

    & git show-ref --verify --quiet "refs/tags/$ReleaseTag"
    if ($LASTEXITCODE -ne 0) {
        throw "Release tag '$ReleaseTag' does not exist in this checkout."
    }
    $tagType = (& git cat-file -t "refs/tags/$ReleaseTag").Trim()
    if ($LASTEXITCODE -ne 0 -or $tagType -cne 'tag') {
        throw "Release tag '$ReleaseTag' must be an annotated tag."
    }
    $resolvedCommit = (& git rev-list -n 1 "refs/tags/$ReleaseTag").Trim()
    if ($LASTEXITCODE -ne 0 -or $resolvedCommit -cne $ExpectedCommit) {
        throw "Release tag '$ReleaseTag' resolves to '$resolvedCommit', not '$ExpectedCommit'."
    }

    if ($packageMetadata.Count -gt 0) {
        $versions = @($packageMetadata.Version | Sort-Object -Unique -CaseSensitive)
        if ($versions.Count -ne 1 -or $versions[0] -cne $releaseVersion) {
            throw "NuGet candidates must all use release version '$releaseVersion'; found '$($versions -join ', ')'."
        }
    }
}

Write-Output "Validated Customer promotion selection: $($nuget.Count) NuGet candidates and $($oci.Count) OCI candidates."
