using BenchmarkDotNet.Attributes;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Diagnostics;
using Csxaml.Tooling.Core.Formatting;

namespace Csxaml.Benchmarks.Scenarios;

[MemoryDiagnoser]
public class ToolingBenchmarks
{
    private readonly CsxamlCompletionService _completionService = new();
    private readonly CsxamlDiagnosticService _diagnosticService = new();
    private readonly CsxamlFormattingService _formattingService = new();
    private string _rootDirectory = string.Empty;
    private string _filePath = string.Empty;
    private string _text = string.Empty;
    private int _tagCompletionPosition;
    private int _attributeCompletionPosition;

    [GlobalSetup]
    public void Setup()
    {
        _rootDirectory = Path.Combine(Path.GetTempPath(), "csxaml-tooling-benchmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootDirectory);
        File.WriteAllText(Path.Combine(_rootDirectory, "BenchmarkApp.csproj"), CreateProjectFile());

        _filePath = Path.Combine(_rootDirectory, "BenchPage.csxaml");
        _text = CreateDocument();
        File.WriteAllText(_filePath, _text);
        _tagCompletionPosition = _text.IndexOf("<But", StringComparison.Ordinal) + 4;
        _attributeCompletionPosition = _text.IndexOf("OnC", StringComparison.Ordinal) + 3;
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
    public int TagCompletion()
    {
        return _completionService.GetCompletions(_filePath, _text, _tagCompletionPosition).Count;
    }

    [Benchmark]
    public int AttributeCompletion()
    {
        return _completionService.GetCompletions(_filePath, _text, _attributeCompletionPosition).Count;
    }

    [Benchmark]
    public int Diagnostics()
    {
        return _diagnosticService.GetDiagnostics(_filePath, _text).Count;
    }

    [Benchmark]
    public int FormatDocument()
    {
        return _formattingService.FormatDocument(_text).Length;
    }

    private static string CreateProjectFile()
    {
        return """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
                <RootNamespace>BenchmarkApp</RootNamespace>
                <EnableCsxaml>true</EnableCsxaml>
              </PropertyGroup>
            </Project>
            """;
    }

    private static string CreateDocument()
    {
        return """
            namespace BenchmarkApp;

            component Element BenchPage {
                State<int> Count = new State<int>(0);

                render <StackPanel Spacing={8}>
                    <TextBlock Text={$"Count: {Count.Value}"} />
                    <But />
                    <Button OnC />
                </StackPanel>;
            }
            """;
    }
}
