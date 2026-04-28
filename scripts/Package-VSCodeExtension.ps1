Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "PackageValidation\PackageValidationSupport.ps1")

function Resolve-RequiredCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Names
    )

    foreach ($name in $Names) {
        $command = Get-Command -Name $name -ErrorAction SilentlyContinue
        if ($null -ne $command) {
            return $command.Source
        }
    }

    throw "Required command was not found. Tried: $($Names -join ', '). Install Node.js 20+ and ensure npm/npx are available on PATH before packaging the VS Code extension."
}

function Invoke-ExternalCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

function Copy-DirectoryContents {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,
        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory,
        [Parameter(Mandatory = $true)]
        [string[]]$ExcludedNames
    )

    New-Item -ItemType Directory -Force -Path $DestinationDirectory | Out-Null

    Get-ChildItem -LiteralPath $SourceDirectory -Force | Where-Object {
        $_.Name -notin $ExcludedNames
    } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $DestinationDirectory -Recurse -Force
    }
}

function Update-ExtensionManifestVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExtensionDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $packageJsonPath = Join-Path $ExtensionDirectory "package.json"
    $packageJson = Get-Content -Path $packageJsonPath -Raw | ConvertFrom-Json
    $packageJson.version = $Version
    $packageJson.preview = $Version.Contains("-")
    $packageJson | ConvertTo-Json -Depth 100 | Set-Content -Path $packageJsonPath -NoNewline

    $packageLockPath = Join-Path $ExtensionDirectory "package-lock.json"
    $packageLockLines = [System.Collections.Generic.List[string]]::new()
    $packageLockLines.AddRange([string[]](Get-Content -Path $packageLockPath))

    $updatedCount = 0
    for ($index = 0; $index -lt $packageLockLines.Count; $index++) {
        if ($packageLockLines[$index] -notmatch '^\s*"version":\s*"') {
            continue
        }

        $packageLockLines[$index] = ($packageLockLines[$index] -replace '("version"\s*:\s*")[^"]+(")', "`${1}$Version`${2}")
        $updatedCount++
        if ($updatedCount -eq 2) {
            break
        }
    }

    if ($updatedCount -lt 2) {
        throw "Could not update the root version fields in $packageLockPath."
    }

    Set-Content -Path $packageLockPath -Value $packageLockLines
}

function Assert-VsixContents {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VsixPath
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $extractDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("csxaml-vsix-" + [Guid]::NewGuid().ToString("N"))
    [System.IO.Compression.ZipFile]::ExtractToDirectory($VsixPath, $extractDirectory)

    try {
        Assert-CsxamlPathExists -Path (Join-Path $extractDirectory "extension\package.json")
        Assert-CsxamlPathExists -Path (Join-Path $extractDirectory "extension\dist\extension.js")
        Assert-CsxamlPathExists -Path (Join-Path $extractDirectory "extension\LanguageServer\Csxaml.LanguageServer.exe")
        Assert-CsxamlPathExists -Path (Join-Path $extractDirectory "extension\node_modules\vscode-languageclient")
        Assert-CsxamlPathExists -Path (Join-Path $extractDirectory "extension\src\languageServerPathResolver.js")
    }
    finally {
        if (Test-Path -LiteralPath $extractDirectory) {
            Remove-Item -LiteralPath $extractDirectory -Recurse -Force
        }
    }
}

$repoRoot = Get-CsxamlRepoRoot -ScriptRoot $PSScriptRoot
$version = Get-CsxamlPackageVersion -RepoRoot $repoRoot
$extensionSourceDirectory = Join-Path $repoRoot "VSCodeExtension"
$stagingRoot = Join-Path $repoRoot "artifacts\vscode-extension"
$stagingDirectory = Join-Path $stagingRoot "staging"
$languageServerOutputDirectory = Join-Path $stagingDirectory "LanguageServer"
$vsixOutputDirectory = Join-Path $repoRoot "artifacts\vscode"
$vsixPath = Join-Path $vsixOutputDirectory "csxaml-vscode-extension-$version.vsix"
$languageServerProject = Join-Path $repoRoot "Csxaml.LanguageServer\Csxaml.LanguageServer.csproj"

$npmCommand = Resolve-RequiredCommand -Names @("npm.cmd", "npm")
$npxCommand = Resolve-RequiredCommand -Names @("npx.cmd", "npx")

if (Test-Path -LiteralPath $stagingRoot) {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $stagingRoot | Out-Null
New-Item -ItemType Directory -Force -Path $vsixOutputDirectory | Out-Null

Copy-DirectoryContents -SourceDirectory $extensionSourceDirectory -DestinationDirectory $stagingDirectory -ExcludedNames @("LanguageServer", "node_modules")
Update-ExtensionManifestVersion -ExtensionDirectory $stagingDirectory -Version $version

Invoke-ExternalCommand -FilePath $npmCommand -Arguments @("ci") -WorkingDirectory $stagingDirectory
Invoke-ExternalCommand -FilePath $npmCommand -Arguments @("run", "bundle") -WorkingDirectory $stagingDirectory
Invoke-ExternalCommand -FilePath "dotnet" -Arguments @(
    "publish",
    $languageServerProject,
    "-c",
    "Release",
    "-o",
    $languageServerOutputDirectory
) -WorkingDirectory $repoRoot

Assert-CsxamlPathExists -Path (Join-Path $languageServerOutputDirectory "Csxaml.LanguageServer.exe")

if (Test-Path -LiteralPath $vsixPath) {
    Remove-Item -LiteralPath $vsixPath -Force
}

Invoke-ExternalCommand -FilePath $npxCommand -Arguments @(
    "vsce",
    "package",
    "--allow-missing-repository",
    "--out",
    $vsixPath
) -WorkingDirectory $stagingDirectory

Assert-CsxamlPathExists -Path $vsixPath
Assert-VsixContents -VsixPath $vsixPath

Write-Host "VS Code extension package created successfully."
Write-Host "VSIX: $vsixPath"
Write-Host "Staging directory: $stagingDirectory"
