namespace Csxaml.Benchmarks;

internal sealed class BenchmarkCardComponent : ComponentInstance<BenchmarkCardProps>
{
    public override Node Render()
    {
        var children = new List<Node>
        {
            CreateTextBlock(Props.Title),
        };

        if (Props.IsDone)
        {
            children.Add(CreateTextBlock("Done"));
        }

        return new NativeElementNode(
            "StackPanel",
            key: null,
            properties: Array.Empty<NativePropertyValue>(),
            events: Array.Empty<NativeEventValue>(),
            children);
    }

    private static NativeElementNode CreateTextBlock(string text)
    {
        return new NativeElementNode(
            "TextBlock",
            key: null,
            properties:
            [
                new NativePropertyValue("Text", text),
            ],
            events: Array.Empty<NativeEventValue>(),
            children: Array.Empty<Node>());
    }
}
