using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Generator;

public class ParseBenchmarks
{
    private Parser _parser = null!;
    private SourceDocument _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parser = new Parser();
        _source = new SourceDocument(
            "TodoBoard.csxaml",
            SourceScenarioLibrary.MediumComponent.Files[0].SourceText);
    }

    [Benchmark]
    public int ParseMediumComponent()
    {
        return _parser.Parse(_source).Component.Parameters.Count;
    }
}
