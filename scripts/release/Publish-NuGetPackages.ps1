[CmdletBinding()]
param(
    [Parameter()]
    [string]$PackageDirectory = "artifacts/packages",

    [Parameter(Mandatory)]
    [string]$ApiKey,

    [Parameter()]
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$packageRoot = if ([IO.Path]::IsPathRooted($PackageDirectory)) {
    $PackageDirectory
}
else {
    Join-Path $repoRoot $PackageDirectory
}

$nupkgs = Get-ChildItem $packageRoot -Filter *.nupkg | Where-Object { $_.Name -notlike "*.snupkg" }
$snupkgs = Get-ChildItem $packageRoot -Filter *.snupkg

if (-not $nupkgs) {
    throw "No .nupkg files were found under '$packageRoot'."
}

foreach ($package in $nupkgs) {
    dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for '$($package.FullName)'."
    }
}

foreach ($package in $snupkgs) {
    dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for '$($package.FullName)'."
    }
}
