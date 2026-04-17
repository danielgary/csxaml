[CmdletBinding()]
param(
    [Parameter()]
    [string]$PackagePath = "",

    [Parameter(Mandatory)]
    [string]$PersonalAccessToken,

    [Parameter()]
    [switch]$PreRelease
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$extensionDirectory = Join-Path $repoRoot "VSCodeExtension"

if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    $PackagePath = Get-ChildItem (Join-Path $repoRoot "artifacts/extensions/vscode") -Filter *.vsix |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
elseif (-not [IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

if (-not (Test-Path $PackagePath)) {
    throw "VS Code extension package '$PackagePath' does not exist."
}

$arguments = @(
    "vsce",
    "publish",
    "--packagePath",
    $PackagePath,
    "--pat",
    $PersonalAccessToken,
    "--skip-duplicate"
)

if ($PreRelease) {
    $arguments += "--pre-release"
}

Push-Location $extensionDirectory
try {
    npm ci
    if ($LASTEXITCODE -ne 0) {
        throw "npm ci failed for the VS Code extension."
    }

    npx @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "vsce publish failed."
    }
}
finally {
    Pop-Location
}
