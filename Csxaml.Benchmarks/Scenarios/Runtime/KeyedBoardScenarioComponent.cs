namespace Csxaml.Benchmarks;

internal sealed class KeyedBoardScenarioComponent : ComponentInstance
{
    private bool _alternateTitle;
    private bool _reversed;

    public KeyedBoardScenarioComponent(int itemCount)
    {
        Items = new State<List<BenchmarkTodoItem>>(
            CreateItems(itemCount),
            InvalidateState,
            ValidateStateWrite);
    }

    public State<List<BenchmarkTodoItem>> Items { get; }

    public void ToggleSingleItemUpdate()
    {
        var targetIndex = Items.Value.Count / 2;
        _alternateTitle = !_alternateTitle;
        Items.Value = Items.Value
            .Select((item, index) => index == targetIndex
                ? item with { Title = _alternateTitle ? $"{item.Title}*" : item.Title.TrimEnd('*') }
                : item)
            .ToList();
    }

    public void ToggleReorder()
    {
        _reversed = !_reversed;
        Items.Value = _reversed
            ? Items.Value.OrderByDescending(item => item.Id, StringComparer.Ordinal).ToList()
            : Items.Value.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
    }

    public override Node Render()
    {
        var children = new List<Node>(Items.Value.Count);
        foreach (var item in Items.Value)
        {
            children.Add(
                new ComponentNode(
                    typeof(BenchmarkCardComponent),
                    new BenchmarkCardProps(item.Title, item.IsDone),
                    renderPositionId: "benchmark-cards",
                    key: item.Id));
        }

        return new NativeElementNode(
            "StackPanel",
            key: null,
            properties: Array.Empty<NativePropertyValue>(),
            events: Array.Empty<NativeEventValue>(),
            children);
    }

    private static List<BenchmarkTodoItem> CreateItems(int itemCount)
    {
        return Enumerable.Range(1, itemCount)
            .Select(index => new BenchmarkTodoItem(
                $"item-{index:D4}",
                $"Task {index}",
                IsDone: index % 3 == 0))
            .ToList();
    }
}
