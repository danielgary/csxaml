[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release",

    [Parameter()]
    [string]$OutputDirectory = "artifacts/packages",

    [Parameter()]
    [string]$Version = ""
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$packageOutputDirectory = Join-Path $repoRoot $OutputDirectory

New-Item -ItemType Directory -Force -Path $packageOutputDirectory | Out-Null

$projects = @(
    "Csxaml.Runtime\Csxaml.Runtime.csproj",
    "Csxaml\Csxaml.csproj"
)

foreach ($project in $projects) {
    $projectPath = Join-Path $repoRoot $project
    Write-Host "Packing $projectPath"
    $packArguments = @(
        "pack",
        $projectPath,
        "-c",
        $Configuration,
        "-o",
        $packageOutputDirectory
    )

    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $packArguments += "/p:CsxamlVersion=$Version"
    }

    dotnet @packArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for '$projectPath'."
    }
}

Write-Host "Packed public packages to '$packageOutputDirectory'."
