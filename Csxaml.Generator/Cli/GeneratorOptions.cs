namespace Csxaml.Generator;

internal sealed record GeneratorOptions(
    string OutputDirectory,
    string AssemblyName,
    string DefaultComponentNamespace,
    string InternalGeneratedNamespace,
    IReadOnlyList<string> ReferencePaths,
    IReadOnlyList<string> InputFiles);
