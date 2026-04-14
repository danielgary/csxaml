namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentTreeCoordinatorAttachedPropertyTests
{
    [TestMethod]
    public void Render_ComponentUsageAttachedProperties_AreMergedOntoRenderedRoot()
    {
        var tree = (NativeElementNode)new ComponentTreeCoordinator(new AttachedUsageHostComponent()).Render();

        Assert.AreEqual("Border", tree.TagName);
        Assert.AreEqual(1, RuntimeTreeHelpers.GetAttachedProperty<int>(tree, "Grid", "Column"));
        Assert.AreEqual(
            "todo-editor",
            RuntimeTreeHelpers.GetAttachedProperty<string>(tree, "AutomationProperties", "AutomationId"));
    }

    [TestMethod]
    public void Render_ComponentUsageAttachedProperties_OverrideRootAttachedProperties()
    {
        var tree = (NativeElementNode)new ComponentTreeCoordinator(new AttachedUsageHostComponent()).Render();

        Assert.AreEqual(
            "Outer Editor",
            RuntimeTreeHelpers.GetAttachedProperty<string>(tree, "AutomationProperties", "Name"));
    }

    private sealed class AttachedUsageHostComponent : ComponentInstance
    {
        public override Node Render()
        {
            return new ComponentNode(
                typeof(AttachedRootComponent),
                null,
                [
                    new NativeAttachedPropertyValue("Grid", "Column", 1, ValueKindHint.Int),
                    new NativeAttachedPropertyValue("AutomationProperties", "Name", "Outer Editor", ValueKindHint.String),
                    new NativeAttachedPropertyValue("AutomationProperties", "AutomationId", "todo-editor", ValueKindHint.String)
                ],
                "attached-root",
                null);
        }
    }
}
