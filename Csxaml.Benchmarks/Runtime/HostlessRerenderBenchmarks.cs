using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Runtime;

public class HostlessRerenderBenchmarks
{
    [Params(100, 1000)]
    public int ItemCount { get; set; }

    private RuntimeScenario<KeyedBoardScenarioComponent> _board = null!;
    private RuntimeScenario<ListDetailScenarioComponent> _editor = null!;

    [GlobalSetup]
    public void Setup()
    {
        _board = RuntimeScenarioFactory.CreateKeyedBoardScenario(ItemCount);
        _board.Render();

        _editor = RuntimeScenarioFactory.CreateListDetailScenario(ItemCount);
        _editor.Render();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _editor.Dispose();
        _board.Dispose();
    }

    [Benchmark]
    public int UpdateOneItemInKeyedList()
    {
        _board.Root.ToggleSingleItemUpdate();
        return _board.UpdatedTreeCount;
    }

    [Benchmark]
    public int ReorderKeyedList()
    {
        _board.Root.ToggleReorder();
        return _board.UpdatedTreeCount;
    }

    [Benchmark]
    public int UpdateSelectedItemInListDetailEditor()
    {
        _editor.Root.ToggleSelectedItemTitle();
        return _editor.UpdatedTreeCount;
    }
}
