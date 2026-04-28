param(
    [string]$SiteRoot = "_site",
    [int]$TimeoutSeconds = 15
)

$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest

$resolvedSiteRoot = Resolve-Path $SiteRoot
$siteRootPath = [System.IO.Path]::GetFullPath($resolvedSiteRoot.Path)
$hrefPattern = [regex]'(?i)href\s*=\s*["'']([^"'']+)["'']'
$urls = New-Object "System.Collections.Generic.HashSet[string]"
$failures = New-Object System.Collections.Generic.List[string]

function Get-ExternalUrl {
    param([string]$Href)

    if (-not (
        $Href.StartsWith("http://", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Href.StartsWith("https://", [System.StringComparison]::OrdinalIgnoreCase))) {
        return $null
    }

    if ($Href.StartsWith(
        "https://github.com/danielgary/csxaml/blob/develop/docs-site/",
        [System.StringComparison]::OrdinalIgnoreCase)) {
        return $null
    }

    $hashIndex = $Href.IndexOf("#", [System.StringComparison]::Ordinal)
    if ($hashIndex -ge 0) {
        return $Href.Substring(0, $hashIndex)
    }

    return $Href
}

function Test-Url {
    param([string]$Url)

    $headers = @{
        "User-Agent" = "csxaml-docs-link-check"
    }

    try {
        Invoke-WebRequest `
            -Uri $Url `
            -Method Head `
            -MaximumRedirection 5 `
            -TimeoutSec $TimeoutSeconds `
            -UseBasicParsing `
            -Headers $headers `
            -ErrorAction Stop `
            | Out-Null

        return
    }
    catch {
        $headError = $_.Exception.Message

        try {
            Invoke-WebRequest `
                -Uri $Url `
                -Method Get `
                -MaximumRedirection 5 `
                -TimeoutSec $TimeoutSeconds `
                -UseBasicParsing `
                -Headers $headers `
                -ErrorAction Stop `
                | Out-Null
        }
        catch {
            $failures.Add("$Url - HEAD failed with '$headError'; GET failed with '$($_.Exception.Message)'")
        }
    }
}

foreach ($file in Get-ChildItem -LiteralPath $siteRootPath -Recurse -Filter *.html) {
    $content = Get-Content -Raw -LiteralPath $file.FullName
    foreach ($match in $hrefPattern.Matches($content)) {
        $url = Get-ExternalUrl -Href $match.Groups[1].Value
        if (-not [string]::IsNullOrWhiteSpace($url)) {
            [void]$urls.Add($url)
        }
    }
}

foreach ($url in $urls) {
    Test-Url -Url $url
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Warning "External docs link issue: $_" }
    Write-Warning "Docs external link check found $($failures.Count) issue(s). This check is report-only."
}
else {
    Write-Host "Docs external link check passed for $($urls.Count) unique URL(s)."
}
