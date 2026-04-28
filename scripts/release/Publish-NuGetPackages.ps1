[CmdletBinding()]
param(
    [Parameter()]
    [string]$PackageDirectory = "artifacts/packages",

    [Parameter(Mandatory)]
    [string]$ApiKey,

    [Parameter()]
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. (Join-Path $PSScriptRoot "NuGetPackageArtifactValidation.ps1")

function Test-NuGetPackageExistsOnSource {
    param(
        [string]$Source,
        [string]$PackageId,
        [string]$PackageVersion
    )

    if (Test-Path $Source -PathType Container) {
        $packagePath = Join-Path $Source "$PackageId.$PackageVersion.nupkg"
        return Test-Path $packagePath
    }

    if ($Source -notmatch "^https://api\.nuget\.org/v3/index\.json/?$") {
        return $false
    }

    $normalizedId = $PackageId.ToLowerInvariant()
    $normalizedVersion = $PackageVersion.ToLowerInvariant()
    $packageUrl = "https://api.nuget.org/v3-flatcontainer/$normalizedId/$normalizedVersion/$normalizedId.$normalizedVersion.nupkg"

    try {
        Invoke-WebRequest -Uri $packageUrl -Method Head -UseBasicParsing -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        if ($_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq 404) {
            return $false
        }

        throw
    }
}

$packageRoot = if ([IO.Path]::IsPathRooted($PackageDirectory)) {
    $PackageDirectory
}
else {
    Join-Path $repoRoot $PackageDirectory
}

Assert-NuGetPackageArtifacts `
    -PackageDirectory $packageRoot `
    -PackagesWithoutSymbolPackage @("Csxaml")

$nupkgs = Get-ChildItem $packageRoot -Filter *.nupkg | Where-Object { $_.Name -notlike "*.snupkg" }
$snupkgs = Get-ChildItem $packageRoot -Filter *.snupkg
$preexistingPackageKeys = @{}

if (-not $nupkgs) {
    throw "No .nupkg files were found under '$packageRoot'."
}

foreach ($package in $nupkgs) {
    $identity = Get-NuGetPackageIdentity -PackagePath $package.FullName
    $packageKey = Get-NuGetPackageKey -PackageId $identity.Id -PackageVersion $identity.Version

    if (Test-NuGetPackageExistsOnSource -Source $Source -PackageId $identity.Id -PackageVersion $identity.Version) {
        Write-Host "Package '$($identity.Id)' version '$($identity.Version)' already exists on '$Source'; skipping package and symbols."
        $preexistingPackageKeys[$packageKey] = $true
        continue
    }

    dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for '$($package.FullName)'."
    }
}

foreach ($package in $snupkgs) {
    $identity = Get-NuGetPackageIdentity -PackagePath $package.FullName
    $packageKey = Get-NuGetPackageKey -PackageId $identity.Id -PackageVersion $identity.Version

    if ($preexistingPackageKeys.ContainsKey($packageKey)) {
        Write-Host "Skipping symbol package '$($package.Name)' because its .nupkg already existed before this publish run."
        continue
    }

    dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for '$($package.FullName)'."
    }
}
