namespace Csxaml.Benchmarks;

internal sealed class ListDetailScenarioComponent : ComponentInstance
{
    private bool _alternateTitle;

    public ListDetailScenarioComponent(int itemCount)
    {
        var items = CreateItems(itemCount);
        Items = new State<List<BenchmarkTodoItem>>(items, InvalidateState, ValidateStateWrite);
        SelectedId = new State<string>(items[0].Id, InvalidateState, ValidateStateWrite);
    }

    public State<List<BenchmarkTodoItem>> Items { get; }

    public State<string> SelectedId { get; }

    public void ToggleSelectedItemTitle()
    {
        var selectedId = SelectedId.Value;
        _alternateTitle = !_alternateTitle;
        Items.Value = Items.Value
            .Select(item => item.Id == selectedId
                ? item with { Title = _alternateTitle ? $"{item.Title}!" : item.Title.TrimEnd('!') }
                : item)
            .ToList();
    }

    public override Node Render()
    {
        var selected = GetSelectedItem();
        var listChildren = Items.Value
            .Select(item => (Node)new NativeElementNode(
                "TextBlock",
                key: item.Id,
                properties:
                [
                    new NativePropertyValue("Text", item.Title),
                ],
                events: Array.Empty<NativeEventValue>(),
                children: Array.Empty<Node>()))
            .ToArray();

        var detailChildren = new Node[]
        {
            new NativeElementNode(
                "TextBox",
                key: "editor-title",
                properties:
                [
                    new NativePropertyValue("Text", selected.Title),
                ],
                events: Array.Empty<NativeEventValue>(),
                children: Array.Empty<Node>()),
            new NativeElementNode(
                "CheckBox",
                key: "editor-done",
                properties:
                [
                    new NativePropertyValue("IsChecked", selected.IsDone),
                ],
                events: Array.Empty<NativeEventValue>(),
                children: Array.Empty<Node>()),
        };

        return new NativeElementNode(
            "StackPanel",
            key: null,
            properties: Array.Empty<NativePropertyValue>(),
            events: Array.Empty<NativeEventValue>(),
            children:
            [
                new NativeElementNode(
                    "StackPanel",
                    key: "items",
                    properties: Array.Empty<NativePropertyValue>(),
                    events: Array.Empty<NativeEventValue>(),
                    listChildren),
                new NativeElementNode(
                    "StackPanel",
                    key: "detail",
                    properties: Array.Empty<NativePropertyValue>(),
                    events: Array.Empty<NativeEventValue>(),
                    detailChildren),
            ]);
    }

    private BenchmarkTodoItem GetSelectedItem()
    {
        return Items.Value.First(item => item.Id == SelectedId.Value);
    }

    private static List<BenchmarkTodoItem> CreateItems(int itemCount)
    {
        return Enumerable.Range(1, itemCount)
            .Select(index => new BenchmarkTodoItem(
                $"item-{index:D4}",
                $"Task {index}",
                IsDone: index % 4 == 0))
            .ToList();
    }
}
