namespace Csxaml.Runtime.Tests;

internal sealed class CardViewComponent : ComponentInstance<CardViewProps>
{
    public CardViewComponent()
    {
        LocalCount = new Csxaml.Runtime.State<int>(0, InvalidateState, ValidateStateWrite);
    }

    public Csxaml.Runtime.State<int> LocalCount { get; }

    private bool IsDone => Props.IsDone;

    private string Label => Props.Label;

    public override Node Render()
    {
        var children = new List<Node>
        {
            CreateTextBlock($"{Label}:{LocalCount.Value}")
        };

        if (IsDone)
        {
            children.Add(CreateTextBlock("Done"));
        }

        children.Add(
            new NativeElementNode(
                "Button",
                null,
                [new NativePropertyValue("Content", "Increment")],
                [new NativeEventValue("OnClick", (Action)(() => LocalCount.Value++))],
                Array.Empty<Node>()));

        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            children);
    }

    private static NativeElementNode CreateTextBlock(string text)
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", text)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
