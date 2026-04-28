[CmdletBinding()]
param(
    [Parameter()]
    [string]$PackageDirectory = "artifacts/packages",

    [Parameter()]
    [string]$PackageVersion = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $PSScriptRoot "NuGetPackageArtifactValidation.ps1")

$packageRoot = if ([IO.Path]::IsPathRooted($PackageDirectory)) {
    $PackageDirectory
}
else {
    Join-Path $repoRoot $PackageDirectory
}

Assert-NuGetPackageArtifacts `
    -PackageDirectory $packageRoot `
    -PackageVersion $PackageVersion `
    -PackagesWithoutSymbolPackage @("Csxaml")

Write-Host "NuGet package artifact validation passed for '$packageRoot'."
