namespace Csxaml.Generator;

internal sealed record GeneratorOptions(
    string OutputDirectory,
    IReadOnlyList<string> InputFiles);
