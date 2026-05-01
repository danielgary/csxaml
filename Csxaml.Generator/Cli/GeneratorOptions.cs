namespace Csxaml.Generator;

internal sealed record GeneratorOptions(
    string OutputDirectory,
    string AssemblyName,
    string DefaultComponentNamespace,
    string InternalGeneratedNamespace,
    CsxamlApplicationMode ApplicationMode,
    IReadOnlyList<string> ReferencePaths,
    IReadOnlyList<string> InputFiles);
