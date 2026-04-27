function Get-NuGetSymbolInspectorPath {
    if ($script:NuGetSymbolInspectorPath -and (Test-Path $script:NuGetSymbolInspectorPath)) {
        return $script:NuGetSymbolInspectorPath
    }

    $toolRoot = Join-Path ([IO.Path]::GetTempPath()) "csxaml-nuget-symbol-inspector"
    $projectPath = Join-Path $toolRoot "Csxaml.NuGetSymbolInspector.csproj"
    $programPath = Join-Path $toolRoot "Program.cs"
    $toolPath = Join-Path $toolRoot "bin\Release\net10.0\Csxaml.NuGetSymbolInspector.dll"

    Remove-Item $toolRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $toolRoot | Out-Null

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Csxaml.NuGetSymbolInspector</AssemblyName>
  </PropertyGroup>
</Project>
"@ | Set-Content -Path $projectPath -Encoding UTF8

    @"
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: <assembly-path> <portable-pdb-path>");
    return 2;
}

using var assemblyStream = File.OpenRead(args[0]);
using var peReader = new PEReader(assemblyStream);
var codeViewGuids = peReader
    .ReadDebugDirectory()
    .Where(entry => entry.Type == DebugDirectoryEntryType.CodeView)
    .Select(entry => peReader.ReadCodeViewDebugDirectoryData(entry).Guid)
    .ToArray();

if (codeViewGuids.Length == 0)
{
    Console.Error.WriteLine($"Assembly '{args[0]}' does not contain CodeView debug identity.");
    return 3;
}

using var pdbStream = File.OpenRead(args[1]);
using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
var metadataReader = provider.GetMetadataReader();
var pdbId = metadataReader.DebugMetadataHeader?.Id;

if (pdbId is null || pdbId.Value.Length < 16)
{
    Console.Error.WriteLine($"PDB '{args[1]}' does not contain portable debug identity.");
    return 4;
}

var pdbGuid = new Guid(pdbId.Value.Take(16).ToArray());
if (!codeViewGuids.Contains(pdbGuid))
{
    Console.Error.WriteLine($"PDB '{args[1]}' does not match assembly '{args[0]}'.");
    Console.Error.WriteLine($"Assembly debug IDs: {string.Join(", ", codeViewGuids)}");
    Console.Error.WriteLine($"PDB debug ID: {pdbGuid}");
    return 5;
}

return 0;
"@ | Set-Content -Path $programPath -Encoding UTF8

    dotnet build $projectPath -c Release --nologo | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build NuGet symbol inspector."
    }

    $script:NuGetSymbolInspectorPath = $toolPath
    return $script:NuGetSymbolInspectorPath
}

function Assert-PdbMatchesAssemblyDebugIdentity {
    param(
        [string]$AssemblyPath,
        [string]$PdbPath
    )

    $toolPath = Get-NuGetSymbolInspectorPath
    dotnet $toolPath $AssemblyPath $PdbPath
    if ($LASTEXITCODE -ne 0) {
        throw "PDB '$PdbPath' does not match assembly '$AssemblyPath'."
    }
}
