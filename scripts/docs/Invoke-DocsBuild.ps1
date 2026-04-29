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

function Resolve-RequiredCommand {
    param([string[]]$Names)

    foreach ($name in $Names) {
        $command = Get-Command -Name $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($command) {
            return $command.Source
        }
    }

    throw "Required command was not found. Tried: $($Names -join ', ')."
}

function Invoke-ExternalCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory = ""
    )

    if ($WorkingDirectory) {
        Push-Location $WorkingDirectory
    }

    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed: $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        if ($WorkingDirectory) {
            Pop-Location
        }
    }
}

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

    $docsPackagePath = ".\docs-site\package.json"
    $npmCommand = $null
    if (Test-Path -LiteralPath $docsPackagePath) {
        $npmCommand = Resolve-RequiredCommand -Names @("npm.cmd", "npm")
        Invoke-ExternalCommand -FilePath $npmCommand -Arguments @("ci") -WorkingDirectory ".\docs-site"
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

    dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docfx build failed."
    }

    if ($npmCommand) {
        Invoke-ExternalCommand -FilePath $npmCommand -Arguments @("run", "highlight:csxaml") -WorkingDirectory ".\docs-site"
    }

    if (-not $Serve) {
        & .\scripts\docs\Test-DocsCodeFences.ps1 -DocsRoot .\docs-site -SiteRoot .\_site
        & .\scripts\docs\Test-DocsRenderedIncludes.ps1 -SiteRoot .\_site
    }

    if (-not $Serve -and -not $SkipLinkCheck) {
        & .\scripts\docs\Test-DocsLinks.ps1 -SiteRoot .\_site
    }

    if (-not $Serve -and -not $SkipExternalLinkCheck) {
        & .\scripts\docs\Test-DocsExternalLinks.ps1 -SiteRoot .\_site
    }

    if ($Serve) {
        dotnet docfx serve .\_site
        if ($LASTEXITCODE -ne 0) {
            throw "docfx serve failed."
        }
    }
}
finally {
    Pop-Location
}
