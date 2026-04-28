[CmdletBinding()]
param(
    [Parameter()]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$solutionPath = Join-Path $repoRoot "Csxaml.sln"
$testProjects = @(
    "Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj",
    "Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj",
    "Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj",
    "Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj",
    "Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj",
    "Csxaml.VisualStudio.Tests\Csxaml.VisualStudio.Tests.csproj"
)

Push-Location $repoRoot
try {
    Write-Host "Restoring solution '$solutionPath'."
    dotnet restore $solutionPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed for '$solutionPath'."
    }

    foreach ($project in $testProjects) {
        Write-Host "Testing '$project'."
        dotnet test $project -c $Configuration --no-restore -m:1 /p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet test failed for '$project'."
        }
    }
}
finally {
    Pop-Location
}
