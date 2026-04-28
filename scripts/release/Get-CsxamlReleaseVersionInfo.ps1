function Get-CsxamlReleaseVersionInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )

    $match = [regex]::Match(
        $Version,
        '^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<label>[0-9A-Za-z.-]+))?$')

    if (-not $match.Success) {
        throw "Release version '$Version' is not valid semantic version text."
    }

    $label = $match.Groups["label"].Value
    $isPreRelease = -not [string]::IsNullOrWhiteSpace($label)
    $sequence = 0

    if ($isPreRelease) {
        $numberMatches = [regex]::Matches($label, '\d+')
        if ($numberMatches.Count -gt 0) {
            $sequence = [int]$numberMatches[$numberMatches.Count - 1].Value
        }
        else {
            $sequence = 1
        }
    }

    $major = $match.Groups["major"].Value
    $minor = $match.Groups["minor"].Value
    $patch = $match.Groups["patch"].Value

    return [PSCustomObject]@{
        ReleaseVersion      = $Version
        PackageVersion      = $Version
        VsCodeVersion       = "$major.$minor.$patch"
        VsCodeIsPreRelease  = $isPreRelease
        VsixVersion         = "$major.$minor.$patch.$sequence"
        InformationalVersion = $Version
    }
}
