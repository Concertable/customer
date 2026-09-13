[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter(Mandatory)]
    [string] $ArchiveDirectory,

    [string] $BuildVersion,

    # Native commands write their stdout into PowerShell's success stream, so a caller that captures
    # this script's output gets docker's chatter mixed in with the results. The results go to a file.
    [Parameter(Mandatory)]
    [string] $ResultPath,

    [switch] $KeepImages
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
    throw 'Refusing to build Customer images from a dirty or untracked working tree because OCI revision metadata must identify the exact content.'
}

if ([string]::IsNullOrWhiteSpace($BuildVersion)) {
    $BuildVersion = "0.0.0-local.$($revision.Substring(0, 12))"
}

$resolvedArchiveDirectory = [System.IO.Path]::GetFullPath($ArchiveDirectory)
if (-not (Test-Path -LiteralPath $resolvedArchiveDirectory -PathType Container)) {
    throw "Archive directory '$resolvedArchiveDirectory' does not exist."
}

$verificationId = [Guid]::NewGuid().ToString('N')
$builtImages = [System.Collections.Generic.List[string]]::new()

# ContainerRegistry defaults to a real registry in Directory.Build.props, so a PublishContainer
# invocation without ContainerArchiveOutputPath pushes rather than writing a file. Every target below
# supplies one, and this is the reason it is not optional.
$targets = @(
    [ordered]@{
        Name = 'customer-web'
        Project = 'src/Concertable.Customer.Web/Concertable.Customer.Web.csproj'
        Assembly = 'Concertable.Customer.Web.dll'
        Runtime = 'Microsoft\.AspNetCore\.App 10\.'
        Repository = 'ghcr.io/concertable/customer-web'
    },
    [ordered]@{
        Name = 'customer-migrations'
        Project = 'src/Concertable.Customer.Migrations/Concertable.Customer.Migrations.csproj'
        Assembly = 'Concertable.Customer.Migrations.dll'
        Runtime = 'Microsoft\.NETCore\.App 10\.'
        Repository = 'ghcr.io/concertable/customer-migrations'
    },
    [ordered]@{
        Name = 'customer-seed-simulator'
        Project = 'src/Seed/Concertable.Customer.Seed.Simulator/Concertable.Customer.Seed.Simulator.csproj'
        Assembly = 'Concertable.Customer.Seed.Simulator.dll'
        Runtime = 'Microsoft\.NETCore\.App 10\.'
        Repository = 'ghcr.io/concertable/customer-seed-simulator'
    }
)

function Get-ImageInspection {
    param([Parameter(Mandatory)][string] $Image)

    $json = & docker image inspect $Image
    if ($LASTEXITCODE -ne 0) {
        throw "Could not inspect image '$Image'."
    }

    return ($json | ConvertFrom-Json)[0]
}

function Assert-ImageTagAvailable {
    param([Parameter(Mandatory)][string] $Image)

    $imageIds = @(& docker image ls --quiet --no-trunc --filter "reference=$Image")
    if ($LASTEXITCODE -ne 0) {
        throw "Could not check whether image tag '$Image' is available."
    }
    if ($imageIds.Count -gt 0) {
        throw "Refusing to overwrite existing image tag '$Image'."
    }
}

function Assert-ImageMetadata {
    param(
        [Parameter(Mandatory)][string] $Image,
        [Parameter(Mandatory)][string] $ExpectedAssembly,
        [Parameter(Mandatory)][string] $ExpectedRuntime
    )

    $inspection = Get-ImageInspection -Image $Image

    if ([string]::IsNullOrWhiteSpace([string] $inspection.Config.User) -or $inspection.Config.User -in @('0', 'root')) {
        throw "Image '$Image' does not declare a non-root user."
    }

    # The SDK entrypoint is the published assembly under its working directory, unlike a Dockerfile
    # build where the assembly name stands alone.
    $entrypoint = @($inspection.Config.Entrypoint)
    if ($entrypoint.Count -ne 2 -or $entrypoint[0] -ne 'dotnet' -or -not $entrypoint[1].EndsWith("/$ExpectedAssembly")) {
        throw "Image '$Image' has unexpected entrypoint '$($entrypoint -join ' ')'."
    }

    if ($inspection.Config.Labels.'org.opencontainers.image.source' -ne $repositoryUrl) {
        throw "Image '$Image' does not identify the canonical Customer repository."
    }
    if ($inspection.Config.Labels.'org.opencontainers.image.revision' -ne $revision) {
        throw "Image '$Image' does not identify source revision '$revision'."
    }
    if ($inspection.Config.Labels.'org.opencontainers.image.version' -ne $BuildVersion) {
        throw "Image '$Image' does not identify build version '$BuildVersion'."
    }

    $configuredEnvironment = @($inspection.Config.Env) -join "`n"
    if ($configuredEnvironment -match 'GITHUB_PACKAGES_TOKEN') {
        throw "Image '$Image' retains the package-token environment variable."
    }

    $runtimeOutput = (& docker run --rm --entrypoint dotnet $Image --list-runtimes) -join "`n"
    if ($LASTEXITCODE -ne 0 -or $runtimeOutput -notmatch $ExpectedRuntime) {
        throw "Image '$Image' does not contain its expected .NET 10 runtime."
    }
}

function Remove-VerifiedImages {
    foreach ($image in $builtImages) {
        $inspection = Get-ImageInspection -Image $image
        # ContainerLabel is an item, so a per-run marker cannot be passed on the command line. The
        # provenance labels identify the run just as narrowly: this revision at this version.
        if ($inspection.Config.Labels.'org.opencontainers.image.revision' -ne $revision -or
            $inspection.Config.Labels.'org.opencontainers.image.version' -ne $BuildVersion) {
            throw "Refusing to remove image '$image' because it is not owned by this verification run."
        }

        & docker image rm --force $image
        if ($LASTEXITCODE -ne 0) {
            throw "Could not remove verification image '$image'."
        }
    }
}

try {
    foreach ($target in $targets) {
        $target.Image = "concertable/$($target.Name):verification-$verificationId"
        Assert-ImageTagAvailable -Image $target.Image
    }

    foreach ($target in $targets) {
        $archivePath = Join-Path $resolvedArchiveDirectory "$($target.Name).tar.gz"
        if (Test-Path -LiteralPath $archivePath) {
            throw "Release-candidate archive '$archivePath' already exists."
        }

        $project = Join-Path $repositoryRoot $target.Project
        & dotnet restore $project
        if ($LASTEXITCODE -ne 0) {
            throw "Customer image restore failed for '$($target.Name)' with exit code $LASTEXITCODE."
        }

        & dotnet publish $project `
            --configuration $Configuration `
            --no-restore `
            -t:PublishContainer `
            -p:ContainerRepository="concertable/$($target.Name)" `
            -p:ContainerImageTag="verification-$verificationId" `
            -p:ContainerArchiveOutputPath=$archivePath `
            -p:ConcertableSourceRevision=$revision `
            -p:MinVerVersionOverride=$BuildVersion
        if ($LASTEXITCODE -ne 0) {
            throw "Customer image publish failed for '$($target.Name)' with exit code $LASTEXITCODE."
        }

        if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf) -or (Get-Item -LiteralPath $archivePath).Length -eq 0) {
            throw "Customer image publish produced no archive for '$($target.Name)'."
        }

        # The archive is the artifact; loading it is what makes the image inspectable, and it proves the
        # tarball rather than a separate local build is what the assertions below describe.
        & docker image load --input $archivePath
        if ($LASTEXITCODE -ne 0) {
            throw "Could not load release-candidate archive for '$($target.Name)'."
        }
        $builtImages.Add($target.Image)

        $target.Archive = $archivePath

        # Asserted here rather than in a later pass so the check runs against the image this
        # iteration just loaded, with nothing in between that could retag or reclaim it.
        Assert-ImageMetadata -Image $target.Image -ExpectedAssembly $target.Assembly -ExpectedRuntime $target.Runtime
    }

    $results = @($targets | ForEach-Object {
        [ordered]@{
            Name = $_.Name
            Repository = $_.Repository
            Image = $_.Image
            Archive = $_.Archive
            LocalImageId = [string] (Get-ImageInspection -Image $_.Image).Id
        }
    })
    [System.IO.File]::WriteAllText($ResultPath, ($results | ConvertTo-Json -Depth 6 -AsArray), [System.Text.UTF8Encoding]::new($false))

    Write-Host "Verified Customer images for revision ${revision}: $(($targets | ForEach-Object { $_.Name }) -join ', ')."
}
finally {
    if (-not $KeepImages -and $builtImages.Count -gt 0) {
        Remove-VerifiedImages
    }
}
