using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Generator;

public class ProjectScenarioBenchmarks
{
    private GeneratorRunner _generator = null!;
    private GeneratorScenarioWorkspace _mediumComponent = null!;
    private GeneratorScenarioWorkspace _multiFileProject = null!;

    [GlobalSetup]
    public void Setup()
    {
        _generator = new GeneratorRunner();
        _mediumComponent = GeneratorScenarioWorkspace.Create(SourceScenarioLibrary.MediumComponent);
        _multiFileProject = GeneratorScenarioWorkspace.Create(SourceScenarioLibrary.MultiFileProject);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _mediumComponent.Dispose();
        _multiFileProject.Dispose();
    }

    [Benchmark]
    public int GenerateMediumComponent()
    {
        return _generator.GenerateFiles(_mediumComponent.CreateOptions()).Count;
    }

    [Benchmark]
    public int GenerateMultiFileProject()
    {
        return _generator.GenerateFiles(_multiFileProject.CreateOptions()).Count;
    }
}
