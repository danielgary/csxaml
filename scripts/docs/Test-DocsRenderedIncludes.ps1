param(
    [string]$SiteRoot = "_site"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$resolvedSiteRoot = Resolve-Path $SiteRoot
$siteRootPath = [System.IO.Path]::GetFullPath($resolvedSiteRoot.Path)
$failures = New-Object System.Collections.Generic.List[string]

function Test-BuiltPage {
    param(
        [string]$RelativePath,
        [string[]]$RequiredMarkers
    )

    $localPath = $RelativePath.Replace("/", [System.IO.Path]::DirectorySeparatorChar)
    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $siteRootPath $localPath))
    if (-not $fullPath.StartsWith($siteRootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        $failures.Add("Rendered include check escaped site root: $RelativePath")
        return
    }

    if (-not (Test-Path -LiteralPath $fullPath)) {
        $failures.Add("Rendered include output is missing: $RelativePath")
        return
    }

    $content = Get-Content -Raw -LiteralPath $fullPath
    if ($content.IndexOf("[!INCLUDE]", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        $failures.Add("DocFX include marker was not rendered in: $RelativePath")
    }

    foreach ($marker in $RequiredMarkers) {
        if ($content.IndexOf($marker, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            $failures.Add("Rendered include marker '$marker' was not found in: $RelativePath")
        }
    }
}

Test-BuiltPage `
    -RelativePath "articles/language/specification.html" `
    -RequiredMarkers @(
        "CSXAML Language Specification",
        "component Element",
        "State&lt;T&gt;",
        "render &lt;"
    )

Test-BuiltPage `
    -RelativePath "articles/language/supported-features.html" `
    -RequiredMarkers @(
        "Supported Feature Matrix",
        "Feature-matrix status mapping",
        "Supported in v1",
        "Not in v1"
    )

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "Docs rendered include failure: $_" }
    throw "Docs rendered include check failed with $($failures.Count) failure(s)."
}

Write-Host "Docs rendered include check passed."
