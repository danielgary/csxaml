namespace Csxaml.Runtime.Tests;

internal sealed class FixedParentComponent : ComponentInstance
{
    public FixedParentComponent()
    {
        Version = new Csxaml.Runtime.State<int>(0, () => RequestRender?.Invoke());
    }

    public Csxaml.Runtime.State<int> Version { get; }

    public override Node Render()
    {
        return new StackPanelNode(
        [
            new TextBlockNode($"Version:{Version.Value}"),
            new ComponentNode(typeof(CardViewComponent), new CardViewProps("Fixed", false), "fixed-child", null)
        ]);
    }
}
