[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackageDirectory
)

$ErrorActionPreference = 'Stop'

$expectedPackageIds = @(
    'Concertable.Customer.Hosting'
    'Concertable.Customer.Review.Contracts'
    'Concertable.Customer.Seed.Contracts'
    'Concertable.Customer.TestKit'
    'Concertable.Customer.Ticket.Contracts'
)

$resolvedPackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$packages = @(Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter '*.nupkg' -File |
    Where-Object { -not $_.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase) })

if ($packages.Count -ne $expectedPackageIds.Count) {
    throw "Expected $($expectedPackageIds.Count) package candidates, found $($packages.Count)."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$metadata = foreach ($package in $packages) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $nuspec = @($archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) })
        if ($nuspec.Count -ne 1) {
            throw "Package '$($package.Name)' contains $($nuspec.Count) nuspec files."
        }

        $stream = $nuspec[0].Open()
        try {
            $document = [System.Xml.XmlDocument]::new()
            $document.Load($stream)
            $metadataNode = $document.DocumentElement.ChildNodes |
                Where-Object { $_.LocalName -eq 'metadata' } |
                Select-Object -First 1
            $idNode = $metadataNode.ChildNodes | Where-Object { $_.LocalName -eq 'id' } | Select-Object -First 1
            $versionNode = $metadataNode.ChildNodes | Where-Object { $_.LocalName -eq 'version' } | Select-Object -First 1
            [pscustomobject]@{ Id = $idNode.InnerText; Version = $versionNode.InnerText }
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }
}

$actualPackageIds = @($metadata.Id | Sort-Object)
$expectedSorted = @($expectedPackageIds | Sort-Object)
if (Compare-Object -ReferenceObject $expectedSorted -DifferenceObject $actualPackageIds) {
    throw "Package candidate IDs do not match the current Customer candidate set: $($actualPackageIds -join ', ')."
}

$versions = @($metadata.Version | Sort-Object -Unique)
if ($versions.Count -ne 1) {
    throw "Customer package candidates must use one version; found $($versions -join ', ')."
}

$consumerDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "concertable-customer-package-consumer-$([Guid]::NewGuid().ToString('N'))"
[System.IO.Directory]::CreateDirectory($consumerDirectory) | Out-Null

try {
    $escapedPackageDirectory = [System.Security.SecurityElement]::Escape($resolvedPackageDirectory)
    $escapedVersion = [System.Security.SecurityElement]::Escape($versions[0])
    $projectPath = Join-Path $consumerDirectory 'Consumer.csproj'
    $configPath = Join-Path $consumerDirectory 'nuget.config'

    [System.IO.File]::WriteAllText($projectPath, @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Concertable.Customer.Hosting" Version="$escapedVersion" />
    <PackageReference Include="Concertable.Customer.Review.Contracts" Version="$escapedVersion" />
    <PackageReference Include="Concertable.Customer.Seed.Contracts" Version="$escapedVersion" />
    <PackageReference Include="Concertable.Customer.TestKit" Version="$escapedVersion" />
    <PackageReference Include="Concertable.Customer.Ticket.Contracts" Version="$escapedVersion" />
  </ItemGroup>
</Project>
"@)

    [System.IO.File]::WriteAllText($configPath, @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="customer-candidates" value="$escapedPackageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/Concertable/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="customer-candidates">
      <package pattern="Concertable.Customer.Hosting" />
      <package pattern="Concertable.Customer.Review.Contracts" />
      <package pattern="Concertable.Customer.Seed.Contracts" />
      <package pattern="Concertable.Customer.TestKit" />
      <package pattern="Concertable.Customer.Ticket.Contracts" />
    </packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
    <packageSource key="github"><package pattern="Concertable.*" /></packageSource>
  </packageSourceMapping>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="Concertable" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
"@)

    $packagesPath = Join-Path $consumerDirectory 'packages'
    & dotnet restore $projectPath --configfile $configPath --packages $packagesPath --no-cache
    if ($LASTEXITCODE -ne 0) {
        throw "Clean package-consumer restore failed with exit code $LASTEXITCODE."
    }

    & dotnet build $projectPath --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Clean package-consumer build failed with exit code $LASTEXITCODE."
    }
}
finally {
    if ([System.IO.Directory]::Exists($consumerDirectory)) {
        [System.IO.Directory]::Delete($consumerDirectory, $true)
    }
}
