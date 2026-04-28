using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Runtime;

public class HostlessInitialRenderBenchmarks
{
    [Params(100, 1000)]
    public int ItemCount { get; set; }

    [Benchmark]
    public NativeNode RenderFlatList()
    {
        using var scenario = RuntimeScenarioFactory.CreateFlatListScenario(ItemCount);
        return scenario.Render();
    }
}
