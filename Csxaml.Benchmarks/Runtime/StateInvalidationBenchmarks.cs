using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Runtime;

public class StateInvalidationBenchmarks
{
    private RuntimeScenario<StateSemanticsScenarioComponent> _scenario = null!;

    [GlobalSetup]
    public void Setup()
    {
        _scenario = RuntimeScenarioFactory.CreateStateSemanticsScenario();
        _scenario.Render();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _scenario.Dispose();
    }

    [Benchmark]
    public int EqualAssignmentNoOp()
    {
        _scenario.Root.AssignSameValue();
        return _scenario.UpdatedTreeCount;
    }

    [Benchmark]
    public int TouchRerender()
    {
        _scenario.Root.TouchVersion();
        return _scenario.UpdatedTreeCount;
    }
}
