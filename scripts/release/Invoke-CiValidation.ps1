[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")

Push-Location $repoRoot
try {
    & (Join-Path $repoRoot "scripts\release\Test-ReleaseAutomation.ps1")

    & (Join-Path $repoRoot "scripts\release\Invoke-TestSuite.ps1") -Configuration $Configuration

    & (Join-Path $repoRoot "scripts\release\Pack-PublicPackages.ps1") -Configuration $Configuration
    & (Join-Path $repoRoot "scripts\release\Test-PublicPackageInstall.ps1")
    & (Join-Path $repoRoot "scripts\release\Pack-VsCodeExtension.ps1") -Configuration $Configuration
    & (Join-Path $repoRoot "scripts\release\Pack-VisualStudioExtension.ps1") -Configuration $Configuration
}
finally {
    Pop-Location
}
