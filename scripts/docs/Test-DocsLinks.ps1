param(
    [string]$SiteRoot = "_site",
    [string]$BasePath = "/csxaml/"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$resolvedSiteRoot = Resolve-Path $SiteRoot
$siteRootPath = [System.IO.Path]::GetFullPath($resolvedSiteRoot.Path)
$hrefPattern = [regex]'(?i)href\s*=\s*["'']([^"'']+)["'']'
$failures = New-Object System.Collections.Generic.List[string]

function Test-SkippedHref {
    param([string]$Href)

    return $Href.StartsWith("#", [System.StringComparison]::Ordinal) -or
        $Href.StartsWith("http://", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Href.StartsWith("https://", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Href.StartsWith("mailto:", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Href.StartsWith("javascript:", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Href.StartsWith("data:", [System.StringComparison]::OrdinalIgnoreCase)
}

function Split-Href {
    param([string]$Href)

    $pathAndQuery = $Href
    $fragment = ""
    $hashIndex = $pathAndQuery.IndexOf("#", [System.StringComparison]::Ordinal)
    if ($hashIndex -ge 0) {
        $fragment = $pathAndQuery.Substring($hashIndex + 1)
        $pathAndQuery = $pathAndQuery.Substring(0, $hashIndex)
    }

    $queryIndex = $pathAndQuery.IndexOf("?", [System.StringComparison]::Ordinal)
    if ($queryIndex -ge 0) {
        $pathAndQuery = $pathAndQuery.Substring(0, $queryIndex)
    }

    return [pscustomobject]@{
        Path = $pathAndQuery
        Fragment = $fragment
    }
}

function Resolve-DocsHrefPath {
    param(
        [string]$CurrentDirectory,
        [string]$HrefPath
    )

    $localPath = $HrefPath.Replace("/", [System.IO.Path]::DirectorySeparatorChar)
    if ($HrefPath.StartsWith("/", [System.StringComparison]::Ordinal)) {
        $trimmedBasePath = $BasePath.TrimEnd("/")
        if ($trimmedBasePath.Length -gt 0 -and
            $HrefPath.StartsWith($trimmedBasePath, [System.StringComparison]::OrdinalIgnoreCase)) {
            $localPath = $HrefPath.Substring($trimmedBasePath.Length).TrimStart("/")
        }
        else {
            $localPath = $HrefPath.TrimStart("/")
        }

        $localPath = $localPath.Replace("/", [System.IO.Path]::DirectorySeparatorChar)
        return [System.IO.Path]::GetFullPath((Join-Path $siteRootPath $localPath))
    }

    return [System.IO.Path]::GetFullPath((Join-Path $CurrentDirectory $localPath))
}

function Test-Fragment {
    param(
        [string]$TargetPath,
        [string]$Fragment
    )

    if ([string]::IsNullOrWhiteSpace($Fragment)) {
        return $true
    }

    $decodedFragment = [System.Uri]::UnescapeDataString($Fragment)
    $escapedFragment = [System.Net.WebUtility]::HtmlEncode($decodedFragment)
    $content = Get-Content -Raw -LiteralPath $TargetPath
    return (Test-ContentContains -Content $content -Value "id=""$decodedFragment""") -or
        (Test-ContentContains -Content $content -Value "name=""$decodedFragment""") -or
        (Test-ContentContains -Content $content -Value "id=""$escapedFragment""") -or
        (Test-ContentContains -Content $content -Value "name=""$escapedFragment""")
}

function Test-ContentContains {
    param(
        [string]$Content,
        [string]$Value
    )

    return $Content.IndexOf($Value, [System.StringComparison]::Ordinal) -ge 0
}

foreach ($file in Get-ChildItem -LiteralPath $siteRootPath -Recurse -Filter *.html) {
    $content = Get-Content -Raw -LiteralPath $file.FullName
    $currentDirectory = Split-Path -Parent $file.FullName

    foreach ($match in $hrefPattern.Matches($content)) {
        $href = $match.Groups[1].Value
        if (Test-SkippedHref -Href $href) {
            continue
        }

        $parts = Split-Href -Href $href
        if ([string]::IsNullOrWhiteSpace($parts.Path)) {
            continue
        }

        $targetPath = Resolve-DocsHrefPath -CurrentDirectory $currentDirectory -HrefPath $parts.Path
        if ($targetPath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
            $targetPath = Join-Path $targetPath "index.html"
        }

        if (-not $targetPath.StartsWith($siteRootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            $failures.Add("$($file.FullName): link escapes site root: $href")
            continue
        }

        if (-not (Test-Path -LiteralPath $targetPath)) {
            $failures.Add("$($file.FullName): missing link target: $href")
            continue
        }

        if (-not (Test-Fragment -TargetPath $targetPath -Fragment $parts.Fragment)) {
            $failures.Add("$($file.FullName): missing fragment '#$($parts.Fragment)' for link: $href")
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "Broken docs link: $_" }
    throw "Docs link check failed with $($failures.Count) broken link(s)."
}

Write-Host "Docs link check passed."
