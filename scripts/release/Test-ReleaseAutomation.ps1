[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $repoRoot "scripts\release\Get-CsxamlAutomatedReleasePlan.ps1")

function Assert-Equal {
    param(
        [Parameter(Mandatory)]
        $Actual,

        [Parameter(Mandatory)]
        $Expected,

        [Parameter(Mandatory)]
        [string]$Message
    )

    if ($Actual -ne $Expected) {
        throw "$Message Expected '$Expected' but got '$Actual'."
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed."
    }
}

function New-TestCommit {
    param(
        [Parameter(Mandatory)]
        [string]$FileName,

        [Parameter(Mandatory)]
        [string]$CommitMessage
    )

    Set-Content -Path $FileName -Value $CommitMessage -Encoding UTF8
    Invoke-Git @("add", $FileName)
    Invoke-Git @("commit", "-m", $CommitMessage)
}

function Assert-ReleasePlan {
    param(
        [Parameter(Mandatory)]
        [psobject]$Plan,

        [Parameter(Mandatory)]
        [string]$ExpectedVersion,

        [Parameter(Mandatory)]
        [bool]$ExpectedPreview,

        [Parameter(Mandatory)]
        [bool]$ExpectedShouldRelease
    )

    Assert-Equal $Plan.ReleaseVersion $ExpectedVersion "Unexpected release version."
    Assert-Equal $Plan.IsPreview $ExpectedPreview "Unexpected preview flag."
    Assert-Equal $Plan.ShouldRelease $ExpectedShouldRelease "Unexpected should-release flag."
}

$stable = ConvertTo-CsxamlVersionInfo -TagName "v0.1.0"
Assert-Equal $stable.ReleaseVersion "0.1.0" "Stable tag parsing failed."
Assert-Equal $stable.IsStable $true "Stable tag should be marked stable."

$preview = ConvertTo-CsxamlVersionInfo -TagName "v0.1.0-preview.3"
Assert-Equal $preview.ReleaseVersion "0.1.0-preview.3" "Preview tag parsing failed."
Assert-Equal $preview.PreviewNumber 3 "Preview tag number parsing failed."

$comparison = Compare-CsxamlVersionInfo -Left $preview -Right $stable
Assert-Equal $comparison -1 "Preview release should sort before its matching stable release."

$commitRecords = @(
    [PSCustomObject]@{ Subject = "docs(repo): refresh release notes"; Body = "" },
    [PSCustomObject]@{ Subject = "fix(runtime): preserve retained editor selection"; Body = "" },
    [PSCustomObject]@{ Subject = "feat(generator): support packaged build entrypoint"; Body = "" }
)
Assert-Equal (Get-CsxamlReleaseKindFromCommitRecords -CommitRecords $commitRecords) "minor" "Feature commits should drive a minor release."

$breakingRecords = @(
    [PSCustomObject]@{ Subject = "feat(runtime)!: remove obsolete activation shim"; Body = "" }
)
Assert-Equal (Get-CsxamlReleaseKindFromCommitRecords -CommitRecords $breakingRecords) "major" "Breaking changes should drive a major release."

$patchRecords = @(
    [PSCustomObject]@{ Subject = "perf(runtime): avoid redundant invalidation"; Body = "" }
)
Assert-Equal (Get-CsxamlReleaseKindFromCommitRecords -CommitRecords $patchRecords) "patch" "Perf fixes should drive a patch release."

Assert-Equal (Get-CsxamlNextStableVersion -LatestStableVersion $null -ReleaseKind "minor") "0.1.0" "First public release should start at 0.1.0."
Assert-Equal (Get-CsxamlNextStableVersion -LatestStableVersion (ConvertTo-CsxamlVersionInfo -TagName "v0.1.0") -ReleaseKind "patch") "0.1.1" "Patch bump should increment the patch number."
Assert-Equal (Get-CsxamlNextStableVersion -LatestStableVersion (ConvertTo-CsxamlVersionInfo -TagName "v0.1.0") -ReleaseKind "minor") "0.2.0" "Minor bump should increment the minor number."
Assert-Equal (Get-CsxamlNextStableVersion -LatestStableVersion (ConvertTo-CsxamlVersionInfo -TagName "v0.1.0") -ReleaseKind "major") "1.0.0" "Major bump should increment the major number."

$testRepoRoot = Join-Path ([IO.Path]::GetTempPath()) ("csxaml-release-automation-" + [guid]::NewGuid().ToString("N"))

try {
    New-Item -ItemType Directory -Path $testRepoRoot | Out-Null

    Push-Location $testRepoRoot
    try {
        Invoke-Git @("init", "--initial-branch=master")
        Invoke-Git @("config", "user.name", "CSXAML Test")
        Invoke-Git @("config", "user.email", "csxaml-tests@example.com")

        New-TestCommit -FileName "feature.txt" -CommitMessage "feat(runtime): initial public surface"

        $developPlan = Get-CsxamlAutomatedReleasePlan -BranchName "develop"
        Assert-ReleasePlan -Plan $developPlan -ExpectedVersion "0.1.0-preview.1" -ExpectedPreview $true -ExpectedShouldRelease $true
        Assert-Equal $developPlan.CommitRange "" "First preview should not need a tag-based commit range."

        Invoke-Git @("tag", "v0.1.0-preview.1", "HEAD")

        New-TestCommit -FileName "docs.txt" -CommitMessage "docs(repo): clarify release notes"

        $secondPreviewPlan = Get-CsxamlAutomatedReleasePlan -BranchName "develop"
        Assert-ReleasePlan -Plan $secondPreviewPlan -ExpectedVersion "0.1.0-preview.2" -ExpectedPreview $true -ExpectedShouldRelease $true
        Assert-Equal $secondPreviewPlan.CommitRange "v0.1.0-preview.1..HEAD" "Subsequent preview should diff from the prior preview tag."

        $stablePlan = Get-CsxamlAutomatedReleasePlan -BranchName "master"
        Assert-ReleasePlan -Plan $stablePlan -ExpectedVersion "0.1.0" -ExpectedPreview $false -ExpectedShouldRelease $true

        Invoke-Git @("tag", "v0.1.0", "HEAD")

        New-TestCommit -FileName "fix.txt" -CommitMessage "fix(runtime): tighten release retry path"

        $patchPlan = Get-CsxamlAutomatedReleasePlan -BranchName "master"
        Assert-ReleasePlan -Plan $patchPlan -ExpectedVersion "0.1.1" -ExpectedPreview $false -ExpectedShouldRelease $true
        Assert-Equal $patchPlan.CommitRange "v0.1.0..HEAD" "Stable release should diff from the latest stable tag."
    }
    finally {
        Pop-Location
    }
}
finally {
    Remove-Item $testRepoRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Release automation checks passed."
