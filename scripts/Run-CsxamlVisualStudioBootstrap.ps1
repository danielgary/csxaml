param(
    [string]$Configuration = "Debug",
    [string]$InstanceId = ""
)

$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "VisualStudioBootstrap\VisualStudioBootstrapSupport.ps1")

function Sync-VisualStudioExtensibilityAssemblies
{
    param(
        [string]$SourceRoot,
        [string]$DestinationRoot
    )

    if (-not (Test-Path $SourceRoot))
    {
        throw "Could not find Visual Studio extensibility assemblies at '$SourceRoot'."
    }

    foreach ($fileName in @(
        "Microsoft.VisualStudio.Extensibility.dll",
        "Microsoft.VisualStudio.Extensibility.Contracts.dll",
        "Microsoft.VisualStudio.Extensibility.Framework.dll"))
    {
        Copy-Item -LiteralPath (Join-Path $SourceRoot $fileName) -Destination (Join-Path $DestinationRoot $fileName) -Force
    }

    foreach ($cultureDirectory in Get-ChildItem $SourceRoot -Directory)
    {
        $resourceFiles = Get-ChildItem $cultureDirectory.FullName -Filter "Microsoft.VisualStudio.Extensibility*.resources.dll"
        if (-not $resourceFiles)
        {
            continue
        }

        $destinationCultureDirectory = Join-Path $DestinationRoot $cultureDirectory.Name
        New-Item -ItemType Directory -Path $destinationCultureDirectory -Force | Out-Null
        foreach ($resourceFile in $resourceFiles)
        {
            Copy-Item -LiteralPath $resourceFile.FullName -Destination (Join-Path $destinationCultureDirectory $resourceFile.Name) -Force
        }
    }
}

function Get-DeployedExtensionRoot
{
    param(
        [string]$ExtensionsRoot,
        [string]$ManifestPath
    )

    [xml]$manifest = Get-Content -LiteralPath $ManifestPath
    $namespaceUri = $manifest.DocumentElement.NamespaceURI
    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $namespaceManager.AddNamespace("vsix", $namespaceUri)

    $identityNode = $manifest.SelectSingleNode("/vsix:PackageManifest/vsix:Metadata/vsix:Identity", $namespaceManager)
    $displayNameNode = $manifest.SelectSingleNode("/vsix:PackageManifest/vsix:Metadata/vsix:DisplayName", $namespaceManager)
    if (-not $identityNode -or -not $displayNameNode)
    {
        throw "Could not determine the deployed extension path from '$ManifestPath'."
    }

    $publisher = $identityNode.GetAttribute("Publisher")
    $version = $identityNode.GetAttribute("Version")
    $displayName = $displayNameNode.InnerText.Trim()
    Join-Path $ExtensionsRoot (Join-Path $publisher (Join-Path $displayName $version))
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$dotnetHome = Join-Path $repoRoot ".dotnet-home"
$vsixProject = Join-Path $repoRoot "Csxaml.VisualStudio\Csxaml.VisualStudio.csproj"
$solutionPath = Join-Path $repoRoot "Csxaml.sln"
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"

New-Item -ItemType Directory -Path $dotnetHome -Force | Out-Null
$env:DOTNET_CLI_HOME = $dotnetHome
$env:NUGET_PACKAGES = Join-Path $dotnetHome ".nuget\packages"

$instance = Get-VisualStudioInstance -VsWherePath $vswhere -InstanceId $InstanceId
$devenv = [string]$instance.productPath
$msbuild = Get-MsBuildPath -Instance $instance
$instanceId = [string]$instance.instanceId
$expRoot = Get-ExperimentalRootPath -Instance $instance
$legacyExpRoot = Join-Path $env:LOCALAPPDATA "Microsoft\VisualStudio\Exp"
$previewRoots = @($expRoot, $legacyExpRoot)
$extensionsRoot = Join-Path $expRoot "Extensions"
$deployRoot = Join-Path $extensionsRoot "Csxaml.VisualStudio.Bootstrap"
$stagingRoot = Join-Path $dotnetHome "vsix-staging"
$patchedVsixPath = Join-Path $dotnetHome "Csxaml.VisualStudio.DebugInstall.vsix"
$activityLogPath = Join-Path $expRoot "ActivityLog.xml"
$rootSuffix = "Exp"
$targetSkuId = Get-VisualStudioSkuId -Instance $instance
$targetArchitecture = Get-VisualStudioProductArchitecture
$targetVersionRange = Get-VisualStudioInstallationVersionRange -Instance $instance
$prerequisiteVersionRange = Get-VisualStudioPrerequisiteVersionRange -Instance $instance
$extensibilityAssemblyRoot = Join-Path $instance.installationPath "Common7\IDE\CommonExtensions\Microsoft\Extensibility"

if (-not (Test-Path $devenv))
{
    throw "Could not find devenv.exe for the selected Visual Studio instance."
}

Assert-VisualStudioInstanceClosed -DevenvPath $devenv

Write-Host "Using Visual Studio instance: $($instance.displayName) ($($instance.installationVersion))"
Write-Host "devenv: $devenv"
Write-Host "msbuild: $msbuild"
Write-Host "Target manifest installation target: $targetSkuId ($targetArchitecture) $targetVersionRange"

New-Item -ItemType Directory -Path $expRoot -Force | Out-Null

Write-Host "Building CSXAML Visual Studio extension with full MSBuild..."
Invoke-MsBuild `
    -MsBuildPath $msbuild `
    -ProjectPath $vsixProject `
    -Arguments @(
        "/restore",
        "/t:Build",
        "/p:Configuration=$Configuration",
        "/p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
        "/m:1",
        "/nr:false",
        "/p:UseSharedCompilation=false")

$extensionOutput = Get-ChildItem (Join-Path $repoRoot "Csxaml.VisualStudio\bin\$Configuration") -Recurse -Filter Csxaml.VisualStudio.dll |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $extensionOutput)
{
    throw "Could not find the built extension assembly under Csxaml.VisualStudio\\bin\\$Configuration."
}

Write-Host "Syncing Visual Studio extensibility runtime from $extensibilityAssemblyRoot"
Sync-VisualStudioExtensibilityAssemblies `
    -SourceRoot $extensibilityAssemblyRoot `
    -DestinationRoot (Split-Path -Parent $extensionOutput.FullName)

$vsix = Get-ChildItem (Join-Path $repoRoot "Csxaml.VisualStudio\bin\$Configuration") -Recurse -Filter *.vsix |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $vsix)
{
    throw "Could not find a generated VSIX under Csxaml.VisualStudio\\bin\\$Configuration."
}

$intermediateManifest = Get-ChildItem (Join-Path $repoRoot "Csxaml.VisualStudio\obj\$Configuration") -Recurse -Filter extension.vsixmanifest |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $intermediateManifest)
{
    throw "Could not find the generated intermediate VSIX manifest under Csxaml.VisualStudio\\obj\\$Configuration."
}

$deployedExtensionRoot = Get-DeployedExtensionRoot `
    -ExtensionsRoot $extensionsRoot `
    -ManifestPath $intermediateManifest.FullName

Write-Host "Cleaning legacy manual deployment folder under $deployRoot"
Remove-DirectoryUnderRoot -Path $deployRoot -Root $expRoot

Write-Host "Updating the generated VSIX manifest for the selected Visual Studio instance"
Update-DeployedVsixManifest `
    -ManifestPath $intermediateManifest.FullName `
    -SkuId $targetSkuId `
    -ProductArchitecture $targetArchitecture `
    -VersionRange $targetVersionRange `
    -PrerequisiteVersionRange $prerequisiteVersionRange

Write-Host "Preparing a patched VSIX container for deployment"
New-PatchedVsix `
    -VsixPath $vsix.FullName `
    -ScratchRoot $stagingRoot `
    -PatchedVsixPath $patchedVsixPath `
    -SkuId $targetSkuId `
    -ProductArchitecture $targetArchitecture `
    -VersionRange $targetVersionRange `
    -PrerequisiteVersionRange $prerequisiteVersionRange
Copy-Item -LiteralPath $patchedVsixPath -Destination $vsix.FullName -Force

Write-Host "Enabling preview extensibility features for the experimental hive"
Enable-ExperimentalPreviews -ExperimentalRoots $previewRoots

Write-Host "Clearing Visual Studio experimental caches under $expRoot"
Clear-ExperimentalCaches -ExperimentalRoot $expRoot
Clear-ExtensionMetadataCaches -ExperimentalRoot $expRoot

Write-Host "Deploying the VSIX through Visual Studio's extension deployment target"
Invoke-MsBuild `
    -MsBuildPath $msbuild `
    -ProjectPath $vsixProject `
    -Arguments @(
        "/t:CreateVsixContainer;DeployVsixExtensionFiles",
        "/p:Configuration=$Configuration",
        "/p:DeployExtension=true",
        "/p:DeployTargetInstanceId=$instanceId",
        "/p:VSSDKTargetPlatformRegRootSuffix=$rootSuffix",
        "/m:1",
        "/nr:false")

Write-Host "Syncing Visual Studio extensibility runtime into deployed extension at $deployedExtensionRoot"
Sync-VisualStudioExtensibilityAssemblies `
    -SourceRoot $extensibilityAssemblyRoot `
    -DestinationRoot $deployedExtensionRoot

Write-Host "Refreshing Visual Studio extension configuration..."
& $devenv /RootSuffix $rootSuffix /UpdateConfiguration /Log $activityLogPath
if ($LASTEXITCODE -ne 0)
{
    throw "Visual Studio configuration refresh failed with exit code $LASTEXITCODE."
}

Write-Host "Re-applying preview extensibility settings after configuration refresh"
Enable-ExperimentalPreviews -ExperimentalRoots $previewRoots

Write-Host "Launching Visual Studio experimental instance..."
Start-Process -FilePath $devenv -ArgumentList '/RootSuffix', $rootSuffix, '/Log', $activityLogPath, $solutionPath | Out-Null
Write-Host "Activity log: $activityLogPath"
