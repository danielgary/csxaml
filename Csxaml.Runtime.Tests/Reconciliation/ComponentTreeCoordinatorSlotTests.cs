namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentTreeCoordinatorSlotTests
{
    [TestMethod]
    public void Render_SlottedChildren_RenderInsideWrapper()
    {
        var board = new SlottedBoardComponent(
            [
                new CardItemModel("alpha", "Alpha", false),
                new CardItemModel("beta", "Beta", true)
            ]);
        var root = RuntimeTreeHelpers.RootStackPanel(new ComponentTreeCoordinator(board).Render());

        Assert.AreEqual("Wrapper", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 0), "Text"));
        Assert.AreEqual("Alpha:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(root, 1)));
        Assert.AreEqual("Beta:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(root, 2)));
    }

    [TestMethod]
    public void Render_SlottedNativeChildren_RenderInsideWrapper()
    {
        var root = RuntimeTreeHelpers.RootStackPanel(
            new ComponentTreeCoordinator(new NativeSlotHostComponent()).Render());

        Assert.AreEqual("Wrapper", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 0), "Text"));
        Assert.AreEqual("Native Child", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 1), "Text"));
    }

    [TestMethod]
    public void Render_KeyedSlottedChildren_PreserveStateWhenReordered()
    {
        var board = new SlottedBoardComponent(
            [
                new CardItemModel("alpha", "Alpha", false),
                new CardItemModel("beta", "Beta", false)
            ]);
        var coordinator = new ComponentTreeCoordinator(board);

        var initialRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(initialRoot, 1));

        board.Items.Value =
        [
            new CardItemModel("beta", "Beta", false),
            new CardItemModel("alpha", "Alpha", false)
        ];

        var reorderedRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Beta:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(reorderedRoot, 1)));
        Assert.AreEqual("Alpha:1", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(reorderedRoot, 2)));
    }

    [TestMethod]
    public void Render_WrapperPropChange_PreservesSlottedChildState()
    {
        var host = new WrapperVersionHostComponent();
        var coordinator = new ComponentTreeCoordinator(host);

        var initialRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(initialRoot, 1));

        host.Heading.Value = "Updated Wrapper";

        var updatedRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Updated Wrapper", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(updatedRoot, 0), "Text"));
        Assert.AreEqual("Alpha:1", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(updatedRoot, 1)));
    }

    [TestMethod]
    public void Render_NamedSlotChildren_RenderInsideNamedOutlet()
    {
        var root = RuntimeTreeHelpers.RootStackPanel(
            new ComponentTreeCoordinator(new NamedSlotHostComponent()).Render());

        Assert.AreEqual("Header Child", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 0), "Text"));
        Assert.AreEqual("Default Child", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 1), "Text"));
    }

    private sealed class WrapperVersionHostComponent : ComponentInstance
    {
        public WrapperVersionHostComponent()
        {
            Heading = new Csxaml.Runtime.State<string>("Wrapper", InvalidateState, ValidateStateWrite);
        }

        public Csxaml.Runtime.State<string> Heading { get; }

        public override Node Render()
        {
            return new ComponentNode(
                typeof(SlotWrapperComponent),
                new SlotWrapperProps(Heading.Value),
                [
                    new ComponentNode(
                        typeof(CardViewComponent),
                        new CardViewProps("Alpha", false),
                        "slot-card",
                        "alpha")
                ],
                "wrapper",
                null);
        }
    }

    private sealed class NativeSlotHostComponent : ComponentInstance
    {
        public override Node Render()
        {
            return new ComponentNode(
                typeof(SlotWrapperComponent),
                new SlotWrapperProps("Wrapper"),
                [
                    new NativeElementNode(
                        "TextBlock",
                        null,
                        [new NativePropertyValue("Text", "Native Child")],
                        Array.Empty<NativeEventValue>(),
                        Array.Empty<Node>())
                ],
                "wrapper",
                null);
        }
    }

    private sealed class NamedSlotHostComponent : ComponentInstance
    {
        public override Node Render()
        {
            return new ComponentNode(
                typeof(NamedSlotWrapperComponent),
                null,
                [CreateTextBlock("Default Child")],
                new Dictionary<string, IReadOnlyList<Node>>(StringComparer.Ordinal)
                {
                    ["Header"] = [CreateTextBlock("Header Child")]
                },
                Array.Empty<NativeAttachedPropertyValue>(),
                "wrapper",
                null);
        }
    }

    private sealed class NamedSlotWrapperComponent : ComponentInstance
    {
        public override Node Render()
        {
            var children = new List<Node>();
            children.AddRange(GetNamedSlotContent("Header"));
            children.AddRange(ChildContent);
            return new NativeElementNode(
                "StackPanel",
                null,
                Array.Empty<NativePropertyValue>(),
                Array.Empty<NativeEventValue>(),
                children);
        }
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
