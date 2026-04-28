using BenchmarkDotNet.Attributes;
using Csxaml.Generator;

namespace Csxaml.Benchmarks.Scenarios;

[MemoryDiagnoser]
public class GeneratorBenchmarks
{
    private readonly GeneratorRunner _runner = new();
    private string _rootDirectory = string.Empty;
    private string _outputDirectory = string.Empty;
    private IReadOnlyList<string> _inputFiles = Array.Empty<string>();

    [Params(1, 25, 100, 500)]
    public int ComponentCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rootDirectory = Path.Combine(Path.GetTempPath(), "csxaml-benchmarks", Guid.NewGuid().ToString("N"));
        _outputDirectory = Path.Combine(_rootDirectory, "generated");
        Directory.CreateDirectory(_rootDirectory);

        var files = new List<string>(ComponentCount);
        for (var index = 0; index < ComponentCount; index++)
        {
            var path = Path.Combine(_rootDirectory, $"Bench{index}.csxaml");
            File.WriteAllText(path, CreateComponentSource(index));
            files.Add(path);
        }

        _inputFiles = files;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }

    [Benchmark]
    public int GenerateFiles()
    {
        var options = new GeneratorOptions(
            _outputDirectory,
            "Csxaml.BenchmarkInput",
            "Csxaml.BenchmarkInput",
            "Csxaml.BenchmarkInput.__CsxamlGenerated",
            Array.Empty<string>(),
            _inputFiles);

        return _runner.GenerateFiles(options).Count;
    }

    private static string CreateComponentSource(int index)
    {
        return $$"""
            using Microsoft.UI.Xaml;

            namespace Csxaml.BenchmarkInput;

            component Element Bench{{index}}(string Title) {
                State<int> Count = new State<int>(0);

                string FormatTitle()
                {
                    return $"{Title}:{Count.Value}";
                }

                var labels = new[] { "One", "Two", "Three" };

                render <StackPanel Spacing={8}>
                    <TextBlock Text={FormatTitle()} />
                    if (Count.Value > 0) {
                        <TextBlock Text="Positive" />
                    }
                    foreach (var label in labels) {
                        <TextBlock Key={label} Text={label} />
                    }
                    <Slot />
                </StackPanel>;
            }
            """;
    }
}
