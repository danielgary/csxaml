[CmdletBinding()]
param(
    [Parameter()]
    [string]$Version = "",

    [Parameter()]
    [switch]$Preview,

    [Parameter()]
    [int]$PreviewNumber = 1,

    [Parameter()]
    [string]$CommitRange = "",

    [Parameter()]
    [string]$OutputDirectory = "artifacts/release-prep"
)

$ErrorActionPreference = "Stop"

if ($PreviewNumber -lt 1) {
    throw "PreviewNumber must be at least 1."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $repoRoot "scripts\release\Get-CsxamlReleaseVersionInfo.ps1")

function Invoke-GitCliff {
    param([string[]]$Arguments)

    & git-cliff @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git-cliff failed for arguments: $($Arguments -join ' ')"
    }
}

function Get-BumpedVersion {
    $result = & git-cliff --bumped-version
    if ($LASTEXITCODE -ne 0) {
        throw "git-cliff --bumped-version failed."
    }

    return ($result | Select-Object -Last 1).Trim()
}

$resolvedVersion = if ([string]::IsNullOrWhiteSpace($Version)) {
    Get-BumpedVersion
}
else {
    $Version.Trim()
}

if ($Preview -and $resolvedVersion -notmatch '-') {
    $resolvedVersion = "$resolvedVersion-preview.$PreviewNumber"
}

$releaseInfo = Get-CsxamlReleaseVersionInfo -Version $resolvedVersion
$outputRoot = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
}
else {
    Join-Path $repoRoot $OutputDirectory
}

$rootChangeLogPath = Join-Path $repoRoot "CHANGELOG.md"
$releaseNotesPath = Join-Path $outputRoot "release-notes.md"
$changelogPreviewPath = Join-Path $outputRoot "CHANGELOG.md"
$metadataPath = Join-Path $outputRoot "release-metadata.json"
$versionPath = Join-Path $outputRoot "release-version.txt"

Remove-Item $outputRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$cliffArguments = @()
if (-not [string]::IsNullOrWhiteSpace($CommitRange)) {
    $cliffArguments += $CommitRange
}
else {
    $cliffArguments += "--unreleased"
}

Invoke-GitCliff @($cliffArguments + @("--tag", $releaseInfo.ReleaseVersion, "--output", $releaseNotesPath))
Copy-Item $rootChangeLogPath $changelogPreviewPath -Force
Invoke-GitCliff @($cliffArguments + @("--tag", $releaseInfo.ReleaseVersion, "--prepend", $changelogPreviewPath))

[ordered]@{
    ReleaseVersion     = $releaseInfo.ReleaseVersion
    PackageVersion     = $releaseInfo.PackageVersion
    VsCodeVersion      = $releaseInfo.VsCodeVersion
    VsCodeIsPreRelease = $releaseInfo.VsCodeIsPreRelease
    VsixVersion        = $releaseInfo.VsixVersion
} | ConvertTo-Json | Set-Content -Path $metadataPath -Encoding UTF8

Set-Content -Path $versionPath -Value $releaseInfo.ReleaseVersion -Encoding UTF8

Write-Host "Prepared release artifacts for version '$($releaseInfo.ReleaseVersion)' in '$outputRoot'."
