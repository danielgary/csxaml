using BenchmarkDotNet.Attributes;
using Csxaml.Runtime;

namespace Csxaml.Benchmarks.Scenarios;

[MemoryDiagnoser]
public class RuntimeReconciliationBenchmarks
{
    private KeyedListComponent _component = null!;
    private ComponentTreeCoordinator _coordinator = null!;
    private List<RowModel> _items = [];

    [Params(100, 1000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = CreateItems(ItemCount);
        _component = new KeyedListComponent(_items);
        _coordinator = new ComponentTreeCoordinator(_component);
        _coordinator.Render();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _coordinator.Dispose();
    }

    [Benchmark]
    public NativeNode InitialRender()
    {
        using var coordinator = new ComponentTreeCoordinator(new KeyedListComponent(_items));
        return coordinator.Render();
    }

    [Benchmark]
    public NativeNode KeyedReverseRerender()
    {
        _component.Items = _items.AsEnumerable().Reverse().ToArray();
        return _coordinator.Render();
    }

    [Benchmark]
    public NativeNode MiddleInsertRerender()
    {
        var next = _items.ToList();
        next.Insert(next.Count / 2, new RowModel("inserted", "Inserted", false));
        _component.Items = next;
        return _coordinator.Render();
    }

    private static List<RowModel> CreateItems(int count)
    {
        return Enumerable.Range(0, count)
            .Select(index => new RowModel($"item-{index}", $"Item {index}", index % 2 == 0))
            .ToList();
    }

    private sealed class KeyedListComponent(IReadOnlyList<RowModel> items) : ComponentInstance
    {
        public IReadOnlyList<RowModel> Items { get; set; } = items;

        public override Node Render()
        {
            var children = Items
                .Select(item => new ComponentNode(typeof(RowComponent), item, "rows", item.Id))
                .Cast<Node>()
                .ToArray();

            return new NativeElementNode(
                "StackPanel",
                null,
                Array.Empty<NativePropertyValue>(),
                Array.Empty<NativeEventValue>(),
                children);
        }
    }

    private sealed class RowComponent : ComponentInstance<RowModel>
    {
        public override Node Render()
        {
            var children = new List<Node>
            {
                new NativeElementNode(
                    "TextBlock",
                    null,
                    [new NativePropertyValue("Text", Props.Label)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            };

            if (Props.IsDone)
            {
                children.Add(
                    new NativeElementNode(
                        "TextBlock",
                        null,
                        [new NativePropertyValue("Text", "Done")],
                        Array.Empty<NativeEventValue>(),
                        Array.Empty<Node>()));
            }

            return new NativeElementNode(
                "StackPanel",
                null,
                Array.Empty<NativePropertyValue>(),
                Array.Empty<NativeEventValue>(),
                children);
        }
    }

    private sealed record RowModel(string Id, string Label, bool IsDone);
}
