namespace Csxaml.Benchmarks;

internal sealed class FlatListScenarioComponent : ComponentInstance
{
    private readonly string[] _items;

    public FlatListScenarioComponent(int itemCount)
    {
        _items = Enumerable.Range(1, itemCount)
            .Select(index => $"Item {index}")
            .ToArray();
    }

    public override Node Render()
    {
        var children = new Node[_items.Length];
        for (var i = 0; i < _items.Length; i++)
        {
            children[i] = new NativeElementNode(
                "TextBlock",
                key: i.ToString(),
                properties:
                [
                    new NativePropertyValue("Text", _items[i]),
                ],
                events: Array.Empty<NativeEventValue>(),
                children: Array.Empty<Node>());
        }

        return new NativeElementNode(
            "StackPanel",
            key: null,
            properties: Array.Empty<NativePropertyValue>(),
            events: Array.Empty<NativeEventValue>(),
            children);
    }
}
