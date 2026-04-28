param(
    [string]$Filter = "*"
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\\..")
$artifactsPath = Join-Path $repoRoot "artifacts\\benchmarks"

New-Item -ItemType Directory -Force -Path $artifactsPath | Out-Null

Push-Location $repoRoot
try {
    dotnet run -c Release --project .\\Csxaml.Benchmarks\\Csxaml.Benchmarks.csproj -- --filter $Filter
}
finally {
    Pop-Location
}
