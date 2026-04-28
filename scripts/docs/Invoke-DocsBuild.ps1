param(
    [switch]$Serve,
    [switch]$SkipProjectBuild,
    [switch]$AllowWarnings,
    [switch]$SkipLinkCheck,
    [switch]$SkipExternalLinkCheck
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Push-Location $repoRoot

try {
    $generatedPaths = @(
        ".\_site",
        ".\obj\docfx\api",
        ".\obj\docfx\api-tooling"
    )

    foreach ($generatedPath in $generatedPaths) {
        $fullPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot.Path $generatedPath))
        if (-not $fullPath.StartsWith($repoRoot.Path, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove path outside repo: $fullPath"
        }

        if (Test-Path -LiteralPath $fullPath) {
            Remove-Item -LiteralPath $fullPath -Recurse -Force
        }
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed."
    }

    if (-not $SkipProjectBuild) {
        $projects = @(
            ".\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj",
            ".\Csxaml.Runtime\Csxaml.Runtime.csproj",
            ".\Csxaml.Testing\Csxaml.Testing.csproj",
            ".\Csxaml.Tooling.Core\Csxaml.Tooling.Core.csproj",
            ".\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj"
        )

        foreach ($project in $projects) {
            dotnet restore $project --configfile .\docs-site\NuGet.Config --ignore-failed-sources --force-evaluate
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet restore failed for '$project'."
            }

            dotnet build $project -c Release --no-restore -m:1 -nr:false /p:UseSharedCompilation=false
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet build failed for '$project'."
            }
        }
    }

    $metadataArgs = @("docfx", "metadata", ".\docfx.json", "--noRestore")
    $buildArgs = @("docfx", "build", ".\docfx.json")

    if (-not $AllowWarnings) {
        $metadataArgs += "--warningsAsErrors"
        $buildArgs += "--warningsAsErrors"
    }

    dotnet @metadataArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docfx metadata failed."
    }

    if ($Serve) {
        $buildArgs += "--serve"
    }

    dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docfx build failed."
    }

    if (-not $Serve) {
        & .\scripts\docs\Test-DocsRenderedIncludes.ps1 -SiteRoot .\_site
    }

    if (-not $Serve -and -not $SkipLinkCheck) {
        & .\scripts\docs\Test-DocsLinks.ps1 -SiteRoot .\_site
    }

    if (-not $Serve -and -not $SkipExternalLinkCheck) {
        & .\scripts\docs\Test-DocsExternalLinks.ps1 -SiteRoot .\_site
    }
}
finally {
    Pop-Location
}
