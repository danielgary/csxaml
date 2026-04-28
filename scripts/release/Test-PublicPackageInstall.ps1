[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release",

    [Parameter()]
    [string]$PackageOutputDirectory = "artifacts/packages",

    [Parameter()]
    [string]$ValidationDirectory = "",

    [Parameter()]
    [string]$PackageVersion = ""
)

function Get-CsxamlVersion {
    param([string]$DirectoryBuildPropsPath)

    [xml]$document = Get-Content $DirectoryBuildPropsPath
    $versionNode = $document.Project.PropertyGroup.CsxamlVersion | Select-Object -First 1
    if (-not $versionNode) {
        throw "Could not find CsxamlVersion in '$DirectoryBuildPropsPath'."
    }

    return $versionNode.InnerText
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $PSScriptRoot "NuGetPackageArtifactValidation.ps1")

$packageDirectory = Join-Path $repoRoot $PackageOutputDirectory
$directoryBuildPropsPath = Join-Path $repoRoot "Directory.Build.props"

if ([string]::IsNullOrWhiteSpace($ValidationDirectory)) {
    $ValidationDirectory = Join-Path ([IO.Path]::GetTempPath()) "csxaml-package-validation"
}

$validationRoot = if ([IO.Path]::IsPathRooted($ValidationDirectory)) {
    $ValidationDirectory
}
else {
    Join-Path $repoRoot $ValidationDirectory
}

if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    $PackageVersion = Get-CsxamlVersion -DirectoryBuildPropsPath $directoryBuildPropsPath
}

if (-not (Test-Path $packageDirectory)) {
    throw "Package directory '$packageDirectory' does not exist. Run Pack-PublicPackages.ps1 first."
}

Assert-NuGetPackageArtifacts `
    -PackageDirectory $packageDirectory `
    -PackageVersion $PackageVersion `
    -PackagesWithoutSymbolPackage @("Csxaml")

Remove-Item $validationRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $validationRoot | Out-Null

$nugetConfigPath = Join-Path $validationRoot "NuGet.Config"
$projectPath = Join-Path $validationRoot "PackageValidation.csproj"
$componentPath = Join-Path $validationRoot "HelloCard.csxaml"
$packagesRoot = Join-Path $validationRoot ".packages"

@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$packageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Path $nugetConfigPath -Encoding UTF8

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Csxaml" Version="$PackageVersion" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path $projectPath -Encoding UTF8

@"
component Element HelloCard(string Title) {
    render <StackPanel Spacing={8}>
        <TextBlock Text={Title} />
        <Button Content="Tap" />
    </StackPanel>;
}
"@ | Set-Content -Path $componentPath -Encoding UTF8

Write-Host "Restoring validation project from '$packageDirectory'."
dotnet restore $projectPath --configfile $nugetConfigPath --packages $packagesRoot
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed for package validation."
}

Write-Host "Building validation project using packaged CSXAML targets."
dotnet build $projectPath -c $Configuration --no-restore /p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for package validation."
}

$generatedFile = Get-ChildItem $validationRoot -Recurse -Filter *.g.cs | Select-Object -First 1
if (-not $generatedFile) {
    throw "Package validation build succeeded, but no generated .g.cs file was found."
}

Write-Host "Package validation succeeded. Generated file: $($generatedFile.FullName)"
