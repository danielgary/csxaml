namespace Csxaml.Benchmarks;

internal sealed record GeneratorScenario(
    string Name,
    string AssemblyName,
    string DefaultComponentNamespace,
    IReadOnlyList<GeneratorScenarioFile> Files);
