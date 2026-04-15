namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentTreeCoordinatorReconciliationTests
{
    [TestMethod]
    public void ChangingKey_ReplacesChildInstance()
    {
        var board = new KeyedBoardComponent([new CardItemModel("a", "Alpha", false)]);
        var coordinator = new ComponentTreeCoordinator(board);
        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(firstTree, 0));
        board.Items.Value = [new CardItemModel("b", "Alpha", false)];

        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Alpha:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(secondTree, 0)));
    }

    [TestMethod]
    public void ConditionalNodes_AppearAndDisappearWithPropChanges()
    {
        var board = new KeyedBoardComponent([new CardItemModel("a", "Alpha", false)]);
        var coordinator = new ComponentTreeCoordinator(board);
        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.IsFalse(RuntimeTreeHelpers.HasDoneBadge(RuntimeTreeHelpers.ChildCard(firstTree, 0)));

        board.Items.Value = [new CardItemModel("a", "Alpha", true)];
        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.IsTrue(RuntimeTreeHelpers.HasDoneBadge(RuntimeTreeHelpers.ChildCard(secondTree, 0)));
    }

    [TestMethod]
    public void KeyedChildren_PreserveLocalStateWhenReordered()
    {
        var board = new KeyedBoardComponent(
        [
            new CardItemModel("a", "Alpha", false),
            new CardItemModel("b", "Beta", false)
        ]);

        var coordinator = new ComponentTreeCoordinator(board);
        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(firstTree, 0));
        board.Items.Value =
        [
            new CardItemModel("b", "Beta", false),
            new CardItemModel("a", "Alpha", false)
        ];

        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Beta:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(secondTree, 0)));
        Assert.AreEqual("Alpha:1", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(secondTree, 1)));
    }

    [TestMethod]
    public void RepeatedChildren_FollowCurrentOrdering()
    {
        var board = new KeyedBoardComponent(
        [
            new CardItemModel("a", "Alpha", false),
            new CardItemModel("b", "Beta", false)
        ]);

        var coordinator = new ComponentTreeCoordinator(board);
        board.Items.Value =
        [
            new CardItemModel("b", "Beta", false),
            new CardItemModel("a", "Alpha", false)
        ];

        var tree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Beta:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(tree, 0)));
        Assert.AreEqual("Alpha:0", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(tree, 1)));
    }

    [TestMethod]
    public void DuplicateKeyedComponentSiblings_FailDeterministically()
    {
        var board = new KeyedBoardComponent(
        [
            new CardItemModel("a", "Alpha", false),
            new CardItemModel("a", "Again", false)
        ]);
        var coordinator = new ComponentTreeCoordinator(board);

        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(() => coordinator.Render());

        StringAssert.Contains(error.Message, "cannot share the key 'a'");
    }
}
