namespace Csxaml.Runtime.Tests;

internal sealed class SlottedBoardComponent : ComponentInstance
{
    public SlottedBoardComponent(IReadOnlyList<CardItemModel> items)
    {
        Items = new Csxaml.Runtime.State<List<CardItemModel>>(items.ToList(), () => RequestRender?.Invoke());
    }

    public Csxaml.Runtime.State<List<CardItemModel>> Items { get; }

    public override Node Render()
    {
        var childContent = new List<Node>();
        foreach (var item in Items.Value)
        {
            childContent.Add(
                new ComponentNode(
                    typeof(CardViewComponent),
                    new CardViewProps(item.Label, item.IsDone),
                    "slot-card",
                    item.Id));
        }

        return new ComponentNode(
            typeof(SlotWrapperComponent),
            new SlotWrapperProps("Wrapper"),
            childContent,
            "wrapper",
            null);
    }
}
