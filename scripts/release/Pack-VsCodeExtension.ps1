[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release",

    [Parameter()]
    [string]$OutputDirectory = "artifacts/extensions/vscode",

    [Parameter()]
    [string]$Version = ""
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $repoRoot "scripts\release\Get-CsxamlReleaseVersionInfo.ps1")
$extensionDirectory = Join-Path $repoRoot "VSCodeExtension"
$languageServerProject = Join-Path $repoRoot "Csxaml.LanguageServer\Csxaml.LanguageServer.csproj"
$languageServerOutput = Join-Path $repoRoot "Csxaml.LanguageServer\bin\$Configuration\net10.0"
$packagedLanguageServerDirectory = Join-Path $extensionDirectory "LanguageServer"
$extensionOutputDirectory = Join-Path $repoRoot $OutputDirectory
$packageJsonPath = Join-Path $extensionDirectory "package.json"
$originalPackageJson = $null
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

New-Item -ItemType Directory -Force -Path $extensionOutputDirectory | Out-Null

Push-Location $extensionDirectory
try {
    npm ci
    if ($LASTEXITCODE -ne 0) {
        throw "npm ci failed for the VS Code extension."
    }

    dotnet build $languageServerProject -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed for Csxaml.LanguageServer."
    }

    Remove-Item $packagedLanguageServerDirectory -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $packagedLanguageServerDirectory | Out-Null
    Copy-Item (Join-Path $languageServerOutput "*") $packagedLanguageServerDirectory -Recurse -Force

    $vsceArguments = @("vsce", "package")
    $manifest = Get-Content $packageJsonPath -Raw | ConvertFrom-Json

    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $releaseInfo = Get-CsxamlReleaseVersionInfo -Version $Version
        $originalPackageJson = [System.IO.File]::ReadAllText($packageJsonPath)
        $manifest.version = $releaseInfo.VsCodeVersion
        $updatedManifest = $manifest | ConvertTo-Json -Depth 100
        [System.IO.File]::WriteAllText($packageJsonPath, $updatedManifest, $utf8WithoutBom)

        if ($releaseInfo.VsCodeIsPreRelease) {
            $vsceArguments += "--pre-release"
        }
    }

    $artifactVersion = if ([string]::IsNullOrWhiteSpace($Version)) { $manifest.version } else { $Version }
    $vsixPath = Join-Path $extensionOutputDirectory ("csxaml-vscode-extension-" + $artifactVersion + ".vsix")
    $vsceArguments += @("--out", $vsixPath)

    npx @vsceArguments
    if ($LASTEXITCODE -ne 0) {
        throw "vsce packaging failed."
    }

    Write-Host "VS Code extension packaged to '$vsixPath'."
}
finally {
    if ($null -ne $originalPackageJson) {
        [System.IO.File]::WriteAllText($packageJsonPath, $originalPackageJson, $utf8WithoutBom)
    }

    Remove-Item $packagedLanguageServerDirectory -Recurse -Force -ErrorAction SilentlyContinue
    Pop-Location
}
