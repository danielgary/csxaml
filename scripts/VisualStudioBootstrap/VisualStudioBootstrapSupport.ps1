$ErrorActionPreference = "Stop"
function Get-VisualStudioInstance
{
    param(
        [string]$VsWherePath,
        [string]$InstanceId = ""
    )

    if (-not (Test-Path $VsWherePath))
    {
        throw "Could not find vswhere.exe."
    }

    $instancesJson = (& $VsWherePath -products * -prerelease -format json) -join [Environment]::NewLine
    $instances = [object[]](ConvertFrom-Json $instancesJson)
    $devenvInstances = $instances |
        Where-Object {
            $_ -and
            (Split-Path $_.productPath -Leaf) -ieq 'devenv.exe'
        } |
        Sort-Object { [Version]([string]$_.installationVersion) } -Descending

    if ($InstanceId)
    {
        $requestedInstance = $devenvInstances |
            Where-Object { [string]$_.instanceId -eq $InstanceId } |
            Select-Object -First 1
        if ($requestedInstance)
        {
            return $requestedInstance
        }

        throw "Could not find a Visual Studio instance with instance id '$InstanceId'."
    }

    $supportedInstances = $devenvInstances |
        Where-Object { ([Version]([string]$_.installationVersion)).Major -ge 18 }
    $previewInstance = $supportedInstances |
        Where-Object { $_.isPrerelease } |
        Select-Object -First 1
    if ($previewInstance)
    {
        return $previewInstance
    }

    $releaseInstance = $supportedInstances | Select-Object -First 1
    if ($releaseInstance)
    {
        return $releaseInstance
    }

    $fallbackInstance = $devenvInstances | Select-Object -First 1
    if ($fallbackInstance)
    {
        $displayName = [string]$fallbackInstance.displayName
        $installationVersion = [string]$fallbackInstance.installationVersion
        throw "Found '$displayName' ($installationVersion), but the CSXAML bootstrap currently targets Visual Studio 2026 / version 18.x. Install Visual Studio 2026 with the Visual Studio extension development workload and rerun the launch config."
    }

    throw "Could not determine a supported Visual Studio 2026 instance with devenv.exe. Install Visual Studio 2026 (Insiders or regular) with the Visual Studio extension development workload and rerun the launch config."
}

function Get-ExperimentalRootPath
{
    param(
        [object]$Instance
    )
    $installationVersion = [string]$Instance.installationVersion
    $instanceId = [string]$Instance.instanceId
    $version = [Version]$installationVersion
    $rootName = "{0}.0_{1}Exp" -f $version.Major, $instanceId
    Join-Path $env:LOCALAPPDATA "Microsoft\VisualStudio\$rootName"
}

function Get-VisualStudioInstallationVersionRange
{
    param(
        [object]$Instance
    )

    $majorVersion = ([Version]([string]$Instance.installationVersion)).Major
    "[{0}.0, {1}.0)" -f $majorVersion, ($majorVersion + 1)
}

function Get-VisualStudioPrerequisiteVersionRange
{
    param(
        [object]$Instance
    )

    $majorVersion = ([Version]([string]$Instance.installationVersion)).Major
    "[{0}.0,)" -f $majorVersion
}

function Get-MsBuildPath
{
    param(
        [object]$Instance
    )

    $installationPath = [string]$Instance.installationPath
    $candidate = Join-Path $installationPath "MSBuild\Current\Bin\MSBuild.exe"

    if (Test-Path $candidate)
    {
        return $candidate
    }

    throw "Could not find MSBuild.exe under '$installationPath'."
}
function Get-VisualStudioSkuId
{
    param(
        [object]$Instance
    )

    $productId = [string]$Instance.productId

    switch -Regex ($productId)
    {
        'Community$' { return 'Microsoft.VisualStudio.Community' }
        'Professional$' { return 'Microsoft.VisualStudio.Pro' }
        'Enterprise$' { return 'Microsoft.VisualStudio.Enterprise' }
        default { throw "Unsupported Visual Studio product id '$productId'." }
    }
}

function Invoke-MsBuild
{
    param(
        [string]$MsBuildPath,
        [string]$ProjectPath,
        [string[]]$Arguments
    )

    & $MsBuildPath $ProjectPath @Arguments
    if ($LASTEXITCODE -ne 0)
    {
        throw "MSBuild failed with exit code $LASTEXITCODE."
    }
}

function Get-VisualStudioProductArchitecture
{
    $osArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture

    switch ($osArchitecture)
    {
        'X64' { return 'amd64' }
        'Arm64' { return 'arm64' }
        default { throw "Unsupported architecture '$osArchitecture'." }
    }
}

function Remove-DirectoryUnderRoot
{
    param(
        [string]$Path,
        [string]$Root
    )

    if (-not (Test-Path $Path))
    {
        return
    }

    $resolvedPath = (Resolve-Path -LiteralPath $Path).Path
    $resolvedRoot = [System.IO.Path]::GetFullPath($Root)
    if (-not $resolvedPath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "Refusing to remove '$resolvedPath' because it is outside '$resolvedRoot'."
    }

    Remove-Item -LiteralPath $resolvedPath -Recurse -Force
}

function Clear-ExperimentalCaches
{
    param(
        [string]$ExperimentalRoot
    )

    foreach ($cacheName in @("ComponentModelCache", "MEFCacheBackup", "TextMateCache"))
    {
        Remove-DirectoryUnderRoot -Path (Join-Path $ExperimentalRoot $cacheName) -Root $ExperimentalRoot
    }
}

function Update-DeployedVsixManifest
{
    param(
        [string]$ManifestPath,
        [string]$SkuId,
        [string]$ProductArchitecture,
        [string]$VersionRange = "",
        [string]$PrerequisiteVersionRange = ""
    )

    if (-not (Test-Path $ManifestPath))
    {
        throw "Could not find the VSIX manifest at '$ManifestPath'."
    }

    [xml]$manifest = Get-Content -LiteralPath $ManifestPath
    $namespaceUri = $manifest.DocumentElement.NamespaceURI
    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $namespaceManager.AddNamespace("vsix", $namespaceUri)

    $installationNode = $manifest.SelectSingleNode("/vsix:PackageManifest/vsix:Installation", $namespaceManager)
    if (-not $installationNode)
    {
        throw "Could not find the <Installation> node in '$ManifestPath'."
    }

    $targetNodes = @($installationNode.SelectNodes("vsix:InstallationTarget", $namespaceManager))
    if ($targetNodes.Count -eq 0)
    {
        throw "Could not find any <InstallationTarget> nodes in '$ManifestPath'."
    }

    $versionRange = $VersionRange
    if (-not $versionRange)
    {
        $versionRange = $targetNodes[0].GetAttribute("Version")
    }
    $anchorNode = $installationNode.SelectSingleNode("vsix:DotnetTargetVersions", $namespaceManager)

    foreach ($targetNode in $targetNodes)
    {
        [void]$installationNode.RemoveChild($targetNode)
    }

    $replacementTarget = $manifest.CreateElement("InstallationTarget", $namespaceUri)
    [void]$replacementTarget.SetAttribute("Id", $SkuId)
    [void]$replacementTarget.SetAttribute("Version", $versionRange)

    $architectureNode = $manifest.CreateElement("ProductArchitecture", $namespaceUri)
    $architectureNode.InnerText = $ProductArchitecture
    [void]$replacementTarget.AppendChild($architectureNode)

    if ($anchorNode)
    {
        [void]$installationNode.InsertBefore($replacementTarget, $anchorNode)
    }
    else
    {
        [void]$installationNode.AppendChild($replacementTarget)
    }

    if ($PrerequisiteVersionRange)
    {
        $prerequisiteNodes = $manifest.SelectNodes("/vsix:PackageManifest/vsix:Prerequisites/vsix:Prerequisite", $namespaceManager)
        foreach ($prerequisiteNode in $prerequisiteNodes)
        {
            [void]$prerequisiteNode.SetAttribute("Version", $PrerequisiteVersionRange)
        }
    }

    $manifest.Save($ManifestPath)
}

function New-PatchedVsix
{
    param(
        [string]$VsixPath,
        [string]$ScratchRoot,
        [string]$PatchedVsixPath,
        [string]$SkuId,
        [string]$ProductArchitecture,
        [string]$VersionRange = "",
        [string]$PrerequisiteVersionRange = ""
    )

    Remove-DirectoryUnderRoot -Path $ScratchRoot -Root (Split-Path -Parent $ScratchRoot)
    New-Item -ItemType Directory -Path $ScratchRoot -Force | Out-Null

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($VsixPath, $ScratchRoot)

    $manifestPath = Join-Path $ScratchRoot "extension.vsixmanifest"
    Update-DeployedVsixManifest `
        -ManifestPath $manifestPath `
        -SkuId $SkuId `
        -ProductArchitecture $ProductArchitecture `
        -VersionRange $VersionRange `
        -PrerequisiteVersionRange $PrerequisiteVersionRange

    if (Test-Path $PatchedVsixPath)
    {
        Remove-Item -LiteralPath $PatchedVsixPath -Force
    }

    [System.IO.Compression.ZipFile]::CreateFromDirectory($ScratchRoot, $PatchedVsixPath)
}

function Enable-ExperimentalPreviews
{
    param(
        [string[]]$ExperimentalRoots
    )

    $encoding = [System.Text.UTF8Encoding]::new($false)

    foreach ($experimentalRoot in $ExperimentalRoots)
    {
        New-Item -ItemType Directory -Path $experimentalRoot -Force | Out-Null
        $sdkPath = Join-Path $experimentalRoot "sdk.txt"
        [System.IO.File]::WriteAllText($sdkPath, "UsePreviews=True", $encoding)
        $sdkValue = (Get-Content -LiteralPath $sdkPath -Raw).Trim()
        Write-Host "Preview mode: $sdkPath => $sdkValue"
    }
}

function Clear-ExtensionMetadataCaches
{
    param(
        [string]$ExperimentalRoot
    )

    $extensionsRoot = Join-Path $ExperimentalRoot "Extensions"
    foreach ($metadataPath in @(
        (Join-Path $extensionsRoot "ExtensionMetadata2.0.mpack"),
        (Join-Path $extensionsRoot "ExtensionMetadataCache.mpack")))
    {
        if (Test-Path $metadataPath)
        {
            Remove-Item -LiteralPath $metadataPath -Force
        }
    }
}

function Assert-VisualStudioInstanceClosed
{
    param(
        [string]$DevenvPath
    )

    $runningInstances = Get-Process devenv -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -and $_.Path -ieq $DevenvPath }

    if ($runningInstances)
    {
        throw "Close the target Visual Studio instance at '$DevenvPath' before rerunning the CSXAML bootstrap."
    }
}
