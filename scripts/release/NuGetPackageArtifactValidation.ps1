if (-not (Get-Command Assert-PdbMatchesAssemblyDebugIdentity -ErrorAction SilentlyContinue)) {
    . (Join-Path $PSScriptRoot "NuGetSymbolDebugIdentity.ps1")
}

function Get-NuGetPackageEntryNames {
    param([string]$PackagePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        return @($archive.Entries | ForEach-Object { $_.FullName })
    }
    finally {
        $archive.Dispose()
    }
}

function Get-NuGetPackageIdentity {
    param([string]$PackagePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $nuspecEntry = $archive.Entries |
            Where-Object { $_.FullName -like "*.nuspec" } |
            Select-Object -First 1

        if (-not $nuspecEntry) {
            throw "Package '$PackagePath' does not contain a nuspec file."
        }

        $reader = [IO.StreamReader]::new($nuspecEntry.Open())
        try {
            [xml]$document = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }

    $namespaceManager = [Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespaceManager.AddNamespace("pkg", $document.DocumentElement.NamespaceURI)

    $idNode = $document.SelectSingleNode("/pkg:package/pkg:metadata/pkg:id", $namespaceManager)
    $versionNode = $document.SelectSingleNode("/pkg:package/pkg:metadata/pkg:version", $namespaceManager)
    $id = $idNode.InnerText
    $version = $versionNode.InnerText
    $symbolPackageType = $document.SelectSingleNode(
        "/pkg:package/pkg:metadata/pkg:packageTypes/pkg:packageType[@name='SymbolsPackage']",
        $namespaceManager)

    if ([string]::IsNullOrWhiteSpace($id) -or [string]::IsNullOrWhiteSpace($version)) {
        throw "Package '$PackagePath' has an incomplete nuspec identity."
    }

    return [PSCustomObject]@{
        Id = $id
        Version = $version
        IsSymbolPackage = $null -ne $symbolPackageType
    }
}

function Get-NuGetPackageKey {
    param(
        [string]$PackageId,
        [string]$PackageVersion
    )

    return "$PackageId|$PackageVersion"
}

function Assert-PackageVersion {
    param(
        [string]$PackagePath,
        [object]$Identity,
        [string]$ExpectedVersion
    )

    if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion) -and $Identity.Version -ne $ExpectedVersion) {
        throw "Package '$PackagePath' has version '$($Identity.Version)', expected '$ExpectedVersion'."
    }
}

function Assert-PackageContainsNoPdbs {
    param([string]$PackagePath)

    $pdbEntries = @(Get-NuGetPackageEntryNames -PackagePath $PackagePath | Where-Object { $_ -like "*.pdb" })
    if ($pdbEntries.Count -gt 0) {
        throw "Package '$PackagePath' should not contain PDB files: $($pdbEntries -join ', ')"
    }
}

function Assert-SymbolPackageExtensions {
    param([string]$SymbolPackagePath)

    $allowedExtensions = @(".nuspec", ".pdb", ".xml", ".psmdcp", ".rels", ".p7s")
    $invalidEntries = @(
        Get-NuGetPackageEntryNames -PackagePath $SymbolPackagePath |
            Where-Object { $allowedExtensions -notcontains [IO.Path]::GetExtension($_) }
    )

    if ($invalidEntries.Count -gt 0) {
        throw "Symbol package '$SymbolPackagePath' contains unsupported files: $($invalidEntries -join ', ')"
    }
}

function Assert-SymbolPackageMatchesBinaries {
    param(
        [string]$PackagePath,
        [string]$SymbolPackagePath
    )

    $binaryEntries = @(Get-NuGetPackageEntryNames -PackagePath $PackagePath | Where-Object { $_ -match "\.(dll|exe)$" })
    $pdbEntries = @(Get-NuGetPackageEntryNames -PackagePath $SymbolPackagePath | Where-Object { $_ -like "*.pdb" })

    if ($pdbEntries.Count -eq 0) {
        throw "Symbol package '$SymbolPackagePath' does not contain any PDB files."
    }

    foreach ($pdbEntry in $pdbEntries) {
        if (-not $pdbEntry.StartsWith("lib/", [StringComparison]::OrdinalIgnoreCase)) {
            throw "Symbol package '$SymbolPackagePath' contains '$pdbEntry' outside the lib folder."
        }

        $entryWithoutExtension = $pdbEntry.Substring(0, $pdbEntry.Length - 4)
        $matchingDll = "$entryWithoutExtension.dll"
        $matchingExe = "$entryWithoutExtension.exe"

        if (($binaryEntries -notcontains $matchingDll) -and ($binaryEntries -notcontains $matchingExe)) {
            throw "Symbol package '$SymbolPackagePath' contains '$pdbEntry' without a matching DLL or EXE in '$PackagePath'."
        }
    }

    Assert-SymbolPackageDebugIdentities -PackagePath $PackagePath -SymbolPackagePath $SymbolPackagePath
}

function Assert-SymbolPackageDebugIdentities {
    param(
        [string]$PackagePath,
        [string]$SymbolPackagePath
    )

    $extractRoot = Join-Path ([IO.Path]::GetTempPath()) "csxaml-symbol-package-validation-$([Guid]::NewGuid().ToString('N'))"
    $packageExtractRoot = Join-Path $extractRoot "package"
    $symbolExtractRoot = Join-Path $extractRoot "symbols"

    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [IO.Compression.ZipFile]::ExtractToDirectory($PackagePath, $packageExtractRoot)
        [IO.Compression.ZipFile]::ExtractToDirectory($SymbolPackagePath, $symbolExtractRoot)

        $pdbEntries = @(Get-NuGetPackageEntryNames -PackagePath $SymbolPackagePath | Where-Object { $_ -like "*.pdb" })
        foreach ($pdbEntry in $pdbEntries) {
            $entryWithoutExtension = $pdbEntry.Substring(0, $pdbEntry.Length - 4)
            $matchingDll = Join-Path $packageExtractRoot "$entryWithoutExtension.dll"
            $matchingExe = Join-Path $packageExtractRoot "$entryWithoutExtension.exe"
            $pdbPath = Join-Path $symbolExtractRoot $pdbEntry

            if (Test-Path $matchingDll) {
                Assert-PdbMatchesAssemblyDebugIdentity -AssemblyPath $matchingDll -PdbPath $pdbPath
            }
            elseif (Test-Path $matchingExe) {
                Assert-PdbMatchesAssemblyDebugIdentity -AssemblyPath $matchingExe -PdbPath $pdbPath
            }
            else {
                throw "Could not find an extracted binary for '$pdbEntry'."
            }
        }
    }
    finally {
        Remove-Item $extractRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Assert-NuGetPackageArtifacts {
    param(
        [string]$PackageDirectory,
        [string]$PackageVersion = "",
        [string[]]$PackagesWithoutSymbolPackage = @()
    )

    if (-not (Test-Path $PackageDirectory)) {
        throw "Package directory '$PackageDirectory' does not exist."
    }

    $nupkgs = @(Get-ChildItem $PackageDirectory -Filter *.nupkg | Where-Object { $_.Name -notlike "*.snupkg" })
    $snupkgs = @(Get-ChildItem $PackageDirectory -Filter *.snupkg)

    if ($nupkgs.Count -eq 0) {
        throw "No .nupkg files were found under '$PackageDirectory'."
    }

    $packagesByKey = @{}
    foreach ($package in $nupkgs) {
        $identity = Get-NuGetPackageIdentity -PackagePath $package.FullName
        Assert-PackageVersion -PackagePath $package.FullName -Identity $identity -ExpectedVersion $PackageVersion
        Assert-PackageContainsNoPdbs -PackagePath $package.FullName

        $key = Get-NuGetPackageKey -PackageId $identity.Id -PackageVersion $identity.Version
        $packagesByKey[$key] = $package.FullName
    }

    foreach ($symbolPackage in $snupkgs) {
        $identity = Get-NuGetPackageIdentity -PackagePath $symbolPackage.FullName
        Assert-PackageVersion -PackagePath $symbolPackage.FullName -Identity $identity -ExpectedVersion $PackageVersion

        if (-not $identity.IsSymbolPackage) {
            throw "Symbol package '$($symbolPackage.FullName)' is missing the SymbolsPackage package type."
        }

        if ($PackagesWithoutSymbolPackage -contains $identity.Id) {
            throw "Package '$($identity.Id)' should not produce a symbol package."
        }

        $key = Get-NuGetPackageKey -PackageId $identity.Id -PackageVersion $identity.Version
        if (-not $packagesByKey.ContainsKey($key)) {
            throw "Symbol package '$($symbolPackage.FullName)' has no matching .nupkg in '$PackageDirectory'."
        }

        Assert-SymbolPackageExtensions -SymbolPackagePath $symbolPackage.FullName
        Assert-SymbolPackageMatchesBinaries -PackagePath $packagesByKey[$key] -SymbolPackagePath $symbolPackage.FullName
    }
}
