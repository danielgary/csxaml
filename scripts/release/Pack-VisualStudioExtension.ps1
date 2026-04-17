[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release",

    [Parameter()]
    [string]$OutputDirectory = "artifacts/extensions/visualstudio",

    [Parameter()]
    [string]$Version = ""
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $repoRoot "scripts\release\Get-CsxamlReleaseVersionInfo.ps1")
$projectPath = Join-Path $repoRoot "Csxaml.VisualStudio\Csxaml.VisualStudio.csproj"
$projectOutputDirectory = Join-Path $repoRoot "Csxaml.VisualStudio\bin\$Configuration\net8.0-windows8.0"
$extensionOutputDirectory = Join-Path $repoRoot $OutputDirectory

New-Item -ItemType Directory -Force -Path $extensionOutputDirectory | Out-Null

$buildArguments = @(
    "build",
    $projectPath,
    "-c",
    $Configuration
)

if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $releaseInfo = Get-CsxamlReleaseVersionInfo -Version $Version
    $buildArguments += @(
        "/p:Version=$($releaseInfo.VsixVersion)",
        "/p:FileVersion=$($releaseInfo.VsixVersion)",
        "/p:AssemblyVersion=$($releaseInfo.VsixVersion)",
        "/p:InformationalVersion=$($releaseInfo.InformationalVersion)"
    )
}

dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for Csxaml.VisualStudio."
}

$vsix = Get-ChildItem $projectOutputDirectory -Filter *.vsix | Select-Object -First 1
if (-not $vsix) {
    throw "Could not find a VSIX under '$projectOutputDirectory'."
}

$destinationName = if ([string]::IsNullOrWhiteSpace($Version)) {
    $vsix.Name
}
else {
    "Csxaml.VisualStudio-$Version.vsix"
}

$destinationPath = Join-Path $extensionOutputDirectory $destinationName
Copy-Item $vsix.FullName $destinationPath -Force

Write-Host "Visual Studio extension copied to '$destinationPath'."
