function ConvertTo-CsxamlVersionInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$TagName
    )

    $match = [regex]::Match(
        $TagName,
        '^v(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-preview\.(?<preview>[1-9]\d*))?$')

    if (-not $match.Success) {
        return $null
    }

    $previewGroup = $match.Groups["preview"]
    $previewNumber = if ($previewGroup.Success) { [int]$previewGroup.Value } else { 0 }
    $major = [int]$match.Groups["major"].Value
    $minor = [int]$match.Groups["minor"].Value
    $patch = [int]$match.Groups["patch"].Value
    $baseVersion = "$major.$minor.$patch"
    $releaseVersion = if ($previewNumber -gt 0) {
        "$baseVersion-preview.$previewNumber"
    }
    else {
        $baseVersion
    }

    [PSCustomObject]@{
        TagName         = $TagName
        Major           = $major
        Minor           = $minor
        Patch           = $patch
        PreviewNumber   = $previewNumber
        BaseVersion     = $baseVersion
        ReleaseVersion  = $releaseVersion
        IsPreview       = $previewNumber -gt 0
        IsStable        = $previewNumber -eq 0
    }
}

function Compare-CsxamlVersionInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [psobject]$Left,

        [Parameter(Mandatory)]
        [psobject]$Right
    )

    foreach ($property in "Major", "Minor", "Patch") {
        if ($Left.$property -lt $Right.$property) {
            return -1
        }

        if ($Left.$property -gt $Right.$property) {
            return 1
        }
    }

    if ($Left.IsPreview -and $Right.IsStable) {
        return -1
    }

    if ($Left.IsStable -and $Right.IsPreview) {
        return 1
    }

    if ($Left.PreviewNumber -lt $Right.PreviewNumber) {
        return -1
    }

    if ($Left.PreviewNumber -gt $Right.PreviewNumber) {
        return 1
    }

    return 0
}

function Sort-CsxamlVersionInfos {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [object[]]$VersionInfos
    )

    if ($VersionInfos.Count -eq 0) {
        return @()
    }

    $sorted = [System.Collections.Generic.List[object]]::new()
    foreach ($info in $VersionInfos) {
        $inserted = $false
        for ($index = 0; $index -lt $sorted.Count; $index++) {
            if ((Compare-CsxamlVersionInfo -Left $info -Right $sorted[$index]) -lt 0) {
                $sorted.Insert($index, $info)
                $inserted = $true
                break
            }
        }

        if (-not $inserted) {
            $sorted.Add($info)
        }
    }

    return @($sorted)
}

function Get-CsxamlCommitRecords {
    [CmdletBinding()]
    param(
        [Parameter()]
        [string]$CommitRange = ""
    )

    $format = "%H%x1f%s%x1f%b%x1e"
    $arguments = @("log", "--format=$format")
    if (-not [string]::IsNullOrWhiteSpace($CommitRange)) {
        $arguments += $CommitRange
    }

    $raw = & git @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git log failed for range '$CommitRange'."
    }

    $records = @()
    foreach ($entry in ($raw -join "`n").Split([char]0x1e, [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $fields = $entry.Split([char]0x1f)
        if ($fields.Length -lt 3) {
            continue
        }

        $records += [PSCustomObject]@{
            Sha     = $fields[0].Trim()
            Subject = $fields[1].Trim()
            Body    = $fields[2].Trim()
        }
    }

    return $records
}

function Get-CsxamlReleaseKindFromCommitRecords {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object[]]$CommitRecords
    )

    $hasPatch = $false
    $hasMinor = $false
    $hasMajor = $false

    foreach ($record in $CommitRecords) {
        $subjectMatch = [regex]::Match(
            $record.Subject,
            '^(?<type>[a-z]+)(?:\([^)]+\))?(?<breaking>!)?:\s+')

        $bodyHasBreaking = $record.Body -match '(?m)^BREAKING CHANGE:'
        if ($bodyHasBreaking -or ($subjectMatch.Success -and $subjectMatch.Groups["breaking"].Success)) {
            $hasMajor = $true
            continue
        }

        if (-not $subjectMatch.Success) {
            continue
        }

        switch ($subjectMatch.Groups["type"].Value) {
            "feat" {
                $hasMinor = $true
            }
            "fix" {
                $hasPatch = $true
            }
            "perf" {
                $hasPatch = $true
            }
        }
    }

    if ($hasMajor) {
        return "major"
    }

    if ($hasMinor) {
        return "minor"
    }

    if ($hasPatch) {
        return "patch"
    }

    return "none"
}

function Get-CsxamlNextStableVersion {
    [CmdletBinding()]
    param(
        [Parameter()]
        [psobject]$LatestStableVersion,

        [Parameter(Mandatory)]
        [string]$ReleaseKind
    )

    if ($ReleaseKind -eq "none") {
        return ""
    }

    if ($null -eq $LatestStableVersion) {
        return "0.1.0"
    }

    $major = $LatestStableVersion.Major
    $minor = $LatestStableVersion.Minor
    $patch = $LatestStableVersion.Patch

    switch ($ReleaseKind) {
        "major" {
            $major += 1
            $minor = 0
            $patch = 0
        }
        "minor" {
            $minor += 1
            $patch = 0
        }
        "patch" {
            $patch += 1
        }
        default {
            throw "Unsupported release kind '$ReleaseKind'."
        }
    }

    return "$major.$minor.$patch"
}

function Get-CsxamlAutomatedReleasePlan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet("develop", "master")]
        [string]$BranchName
    )

    $tagNames = & git tag --list "v*"
    if ($LASTEXITCODE -ne 0) {
        throw "git tag --list failed."
    }

    $versionInfos = @()
    foreach ($tagName in $tagNames) {
        $parsed = ConvertTo-CsxamlVersionInfo -TagName $tagName.Trim()
        if ($null -ne $parsed) {
            $versionInfos += $parsed
        }
    }

    $versionInfos = Sort-CsxamlVersionInfos -VersionInfos $versionInfos
    $stableVersions = @($versionInfos | Where-Object IsStable)
    $latestStable = $stableVersions | Select-Object -Last 1

    $headTags = @(
        (& git tag --points-at HEAD) |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    $headVersionInfos = @(
        $headTags |
            ForEach-Object { ConvertTo-CsxamlVersionInfo -TagName $_ } |
            Where-Object { $null -ne $_ }
    )

    $existingHeadRelease = if ($BranchName -eq "develop") {
        $headVersionInfos | Where-Object IsPreview | Select-Object -Last 1
    }
    else {
        $headVersionInfos | Where-Object IsStable | Select-Object -Last 1
    }

    if ($null -ne $existingHeadRelease) {
        $rangeStartTag = if ($existingHeadRelease.IsPreview) {
            $previousPreview = @(
                $versionInfos |
                    Where-Object {
                        $_.IsPreview -and
                        $_.BaseVersion -eq $existingHeadRelease.BaseVersion -and
                        $_.PreviewNumber -lt $existingHeadRelease.PreviewNumber
                    }
            ) | Select-Object -Last 1

            if ($null -ne $previousPreview) {
                $previousPreview.TagName
            }
            elseif ($null -ne $latestStable) {
                $latestStable.TagName
            }
            else {
                ""
            }
        }
        else {
            $previousStable = @(
                foreach ($stableVersion in $stableVersions) {
                    if ($stableVersion.TagName -eq $existingHeadRelease.TagName) {
                        continue
                    }

                    if ((Compare-CsxamlVersionInfo -Left $stableVersion -Right $existingHeadRelease) -lt 0) {
                        $stableVersion
                    }
                }
            ) | Select-Object -Last 1

            if ($null -ne $previousStable) {
                $previousStable.TagName
            }
            else {
                ""
            }
        }

        $commitRange = if ([string]::IsNullOrWhiteSpace($rangeStartTag)) {
            ""
        }
        else {
            "$rangeStartTag..HEAD"
        }

        return [PSCustomObject]@{
            ShouldRelease       = $true
            BranchName          = $BranchName
            ReleaseVersion      = $existingHeadRelease.ReleaseVersion
            ReleaseTag          = $existingHeadRelease.TagName
            IsPreview           = $existingHeadRelease.IsPreview
            TagAlreadyExists    = $true
            CommitRange         = $commitRange
            ReleaseKind         = "existing"
        }
    }

    $stableRange = if ($null -ne $latestStable) {
        "$($latestStable.TagName)..HEAD"
    }
    else {
        ""
    }

    $releaseKind = Get-CsxamlReleaseKindFromCommitRecords -CommitRecords (Get-CsxamlCommitRecords -CommitRange $stableRange)
    $nextStableVersion = Get-CsxamlNextStableVersion -LatestStableVersion $latestStable -ReleaseKind $releaseKind

    if ([string]::IsNullOrWhiteSpace($nextStableVersion)) {
        return [PSCustomObject]@{
            ShouldRelease       = $false
            BranchName          = $BranchName
            ReleaseVersion      = ""
            ReleaseTag          = ""
            IsPreview           = $BranchName -eq "develop"
            TagAlreadyExists    = $false
            CommitRange         = ""
            ReleaseKind         = "none"
        }
    }

    if ($BranchName -eq "develop") {
        $matchingPreviews = @(
            $versionInfos |
                Where-Object { $_.IsPreview -and $_.BaseVersion -eq $nextStableVersion }
        )

        $nextPreviewNumber = if ($matchingPreviews.Count -gt 0) {
            ($matchingPreviews | Select-Object -Last 1).PreviewNumber + 1
        }
        else {
            1
        }

        $releaseVersion = "$nextStableVersion-preview.$nextPreviewNumber"
        $rangeStartTag = if ($matchingPreviews.Count -gt 0) {
            ($matchingPreviews | Select-Object -Last 1).TagName
        }
        elseif ($null -ne $latestStable) {
            $latestStable.TagName
        }
        else {
            ""
        }
    }
    else {
        $releaseVersion = $nextStableVersion
        $rangeStartTag = if ($null -ne $latestStable) {
            $latestStable.TagName
        }
        else {
            ""
        }
    }

    $commitRange = if ([string]::IsNullOrWhiteSpace($rangeStartTag)) {
        ""
    }
    else {
        "$rangeStartTag..HEAD"
    }

    return [PSCustomObject]@{
        ShouldRelease       = $true
        BranchName          = $BranchName
        ReleaseVersion      = $releaseVersion
        ReleaseTag          = "v$releaseVersion"
        IsPreview           = $BranchName -eq "develop"
        TagAlreadyExists    = $false
        CommitRange         = $commitRange
        ReleaseKind         = $releaseKind
    }
}
