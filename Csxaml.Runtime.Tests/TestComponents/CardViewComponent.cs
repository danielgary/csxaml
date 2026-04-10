namespace Csxaml.Runtime.Tests;

internal sealed class CardViewComponent : ComponentInstance<CardViewProps>
{
    public CardViewComponent()
    {
        LocalCount = new Csxaml.Runtime.State<int>(0, () => RequestRender?.Invoke());
    }

    public Csxaml.Runtime.State<int> LocalCount { get; }

    private bool IsDone => Props.IsDone;

    private string Label => Props.Label;

    public override Node Render()
    {
        var children = new List<Node>
        {
            new TextBlockNode($"{Label}:{LocalCount.Value}")
        };

        if (IsDone)
        {
            children.Add(new TextBlockNode("Done"));
        }

        children.Add(new ButtonNode("Increment", () => LocalCount.Value++));
        return new StackPanelNode(children);
    }
}
