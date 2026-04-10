namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentTreeCoordinatorStateTests
{
    [TestMethod]
    public void FixedChildren_AreReusedAcrossParentRerender()
    {
        var root = new FixedParentComponent();
        var coordinator = new ComponentTreeCoordinator(root);
        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(firstTree, 1));
        root.Version.Value++;

        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Fixed:1", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(secondTree, 1)));
    }

    [TestMethod]
    public void StateInvalidation_TriggersCoordinatorUpdate()
    {
        var root = new FixedParentComponent();
        var coordinator = new ComponentTreeCoordinator(root);
        var updatedTrees = new List<NativeNode>();
        coordinator.TreeUpdated += updatedTrees.Add;

        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        RuntimeTreeHelpers.ClickIncrement(RuntimeTreeHelpers.ChildCard(firstTree, 1));

        Assert.HasCount(2, updatedTrees);

        var latestTree = RuntimeTreeHelpers.RootStackPanel(updatedTrees[^1]);
        Assert.AreEqual("Fixed:1", RuntimeTreeHelpers.CardHeader(RuntimeTreeHelpers.ChildCard(latestTree, 1)));
    }
}
