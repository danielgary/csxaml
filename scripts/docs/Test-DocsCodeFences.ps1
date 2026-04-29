param(
    [string]$DocsRoot = ".\docs-site",
    [string]$SiteRoot = ".\_site"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-MarkdownFiles {
    param([string]$Root)

    $files = @()
    if (Test-Path -LiteralPath $Root) {
        $files += Get-ChildItem -LiteralPath $Root -Recurse -File -Filter *.md |
            Where-Object {
                $_.FullName -notmatch "\\node_modules\\" -and
                $_.FullName -notmatch "\\templates\\"
            }
    }

    $includedFiles = @(
        ".\LANGUAGE-SPEC.md",
        ".\docs\external-control-interop.md"
    )

    foreach ($includedFile in $includedFiles) {
        if (Test-Path -LiteralPath $includedFile) {
            $files += Get-Item -LiteralPath $includedFile
        }
    }

    return $files | Sort-Object -Property FullName -Unique
}

function Test-LikelyCsxamlBlock {
    param(
        [string]$Text,
        [string]$Language,
        [string]$FileName
    )

    if ($Text -cmatch "component\s+Element") { return $true }
    if ($Text -cmatch "render\s+<") { return $true }
    if ($Text -cmatch "(?m)^\s*inject\s+") { return $true }
    if ($Text -cmatch "(?m)^\s*State<") { return $true }
    if ($Text -cmatch "On[A-Za-z]+\s*=") { return $true }
    if ($Text -cmatch "(?m)^\s*</?[A-Z][A-Za-z0-9_.:]*") {
        return $Language -ne "xml" -or $FileName -eq "LANGUAGE-SPEC.md"
    }

    return $false
}

function Test-MarkdownCodeFences {
    param([System.IO.FileInfo[]]$MarkdownFiles)

    $failures = New-Object System.Collections.Generic.List[string]

    foreach ($file in $MarkdownFiles) {
        $lines = @(Get-Content -LiteralPath $file.FullName)
        $inFence = $false
        $fenceLanguage = ""
        $fenceStartLine = 0
        $content = New-Object System.Collections.Generic.List[string]

        for ($index = 0; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -match '^```(\S*)') {
                if (-not $inFence) {
                    $inFence = $true
                    $fenceLanguage = $Matches[1].ToLowerInvariant()
                    $fenceStartLine = $index + 1
                    $content.Clear()
                    continue
                }

                $text = $content -join "`n"
                $isLikelyCsxaml = Test-LikelyCsxamlBlock `
                    -Text $text `
                    -Language $fenceLanguage `
                    -FileName $file.Name

                if ($isLikelyCsxaml -and $fenceLanguage -ne "csxaml") {
                    $failures.Add("$($file.FullName):$fenceStartLine uses '$fenceLanguage' for a CSXAML-looking code block.")
                }

                $inFence = $false
                $fenceLanguage = ""
                $content.Clear()
                continue
            }

            if ($inFence) {
                $content.Add($lines[$index])
            }
        }
    }

    return $failures
}

function Test-BuiltHtmlCodeFences {
    param([string]$Root)

    $failures = New-Object System.Collections.Generic.List[string]
    if (-not (Test-Path -LiteralPath $Root)) {
        $failures.Add("Built site root does not exist: $Root")
        return $failures
    }

    $htmlFiles = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter *.html
    $highlightedCount = 0

    foreach ($htmlFile in $htmlFiles) {
        $html = Get-Content -LiteralPath $htmlFile.FullName -Raw

        if ($html -match '<code[^>]+class="[^"]*(lang|language)-csxaml') {
            $failures.Add("$($htmlFile.FullName) still contains an unprocessed CSXAML code block.")
        }

        $matches = [regex]::Matches($html, 'data-csxaml-highlighted="true"')
        $highlightedCount += $matches.Count
    }

    if ($highlightedCount -eq 0) {
        $failures.Add("Built site contains no highlighted CSXAML code blocks.")
    }

    return $failures
}

$markdownFailures = Test-MarkdownCodeFences -MarkdownFiles (Get-MarkdownFiles -Root $DocsRoot)
$htmlFailures = Test-BuiltHtmlCodeFences -Root $SiteRoot
$failures = @($markdownFailures) + @($htmlFailures)

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "Docs code fence issue: $_" }
    throw "Docs code fence validation failed with $($failures.Count) issue(s)."
}

Write-Host "Docs code fence validation passed."
