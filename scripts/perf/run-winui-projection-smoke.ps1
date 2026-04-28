param(
    [int]$Iterations = 50
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\\..")
$artifactsPath = Join-Path $repoRoot "artifacts\\benchmarks"

New-Item -ItemType Directory -Force -Path $artifactsPath | Out-Null

Push-Location $repoRoot
try {
    dotnet run -c Release --project .\\Csxaml.Benchmarks\\Csxaml.Benchmarks.csproj -- --winui-smoke --iterations $Iterations
}
finally {
    Pop-Location
}
