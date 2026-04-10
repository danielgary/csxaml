namespace Csxaml.Runtime.Tests;

internal sealed class KeyedBoardComponent : ComponentInstance
{
    public KeyedBoardComponent(IReadOnlyList<CardItemModel> items)
    {
        Items = new Csxaml.Runtime.State<List<CardItemModel>>(items.ToList(), () => RequestRender?.Invoke());
    }

    public Csxaml.Runtime.State<List<CardItemModel>> Items { get; }

    public override Node Render()
    {
        var children = new List<Node>();
        foreach (var item in Items.Value)
        {
            children.Add(
                new ComponentNode(
                    typeof(CardViewComponent),
                    new CardViewProps(item.Label, item.IsDone),
                    "todo-cards",
                    item.Id));
        }

        return new StackPanelNode(children);
    }
}
