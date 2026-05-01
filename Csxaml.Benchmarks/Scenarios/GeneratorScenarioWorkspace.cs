namespace Csxaml.Benchmarks;

internal sealed class GeneratorScenarioWorkspace : IDisposable
{
    private readonly string _rootDirectory;

    private GeneratorScenarioWorkspace(
        GeneratorScenario scenario,
        string rootDirectory,
        string outputDirectory,
        IReadOnlyList<string> inputFiles)
    {
        Scenario = scenario;
        _rootDirectory = rootDirectory;
        OutputDirectory = outputDirectory;
        InputFiles = inputFiles;
    }

    public GeneratorScenario Scenario { get; }

    public string OutputDirectory { get; }

    public IReadOnlyList<string> InputFiles { get; }

    public static GeneratorScenarioWorkspace Create(GeneratorScenario scenario)
    {
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "csxaml-benchmarks",
            scenario.Name,
            Guid.NewGuid().ToString("N"));
        var sourceDirectory = Path.Combine(rootDirectory, "src");
        var outputDirectory = Path.Combine(rootDirectory, "generated");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(outputDirectory);

        var inputFiles = new List<string>(scenario.Files.Count);
        foreach (var file in scenario.Files)
        {
            var filePath = Path.Combine(sourceDirectory, file.FileName);
            File.WriteAllText(filePath, file.SourceText);
            inputFiles.Add(filePath);
        }

        return new GeneratorScenarioWorkspace(
            scenario,
            rootDirectory,
            outputDirectory,
            inputFiles);
    }

    public GeneratorOptions CreateOptions()
    {
        return new GeneratorOptions(
            OutputDirectory,
            Scenario.AssemblyName,
            Scenario.DefaultComponentNamespace,
            $"{Scenario.DefaultComponentNamespace}.Generated",
            CsxamlApplicationMode.Hybrid,
            ReferencePaths: Array.Empty<string>(),
            InputFiles);
    }

    public void Dispose()
    {
        if (!Directory.Exists(_rootDirectory))
        {
            return;
        }

        Directory.Delete(_rootDirectory, recursive: true);
    }
}
