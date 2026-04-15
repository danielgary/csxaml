namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentTreeCoordinatorRenderPhaseTests
{
    [TestMethod]
    public void StateWriteDuringRender_FailsWithClearRuntimeError()
    {
        var coordinator = new ComponentTreeCoordinator(new RenderPhaseStateWriteComponent());

        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(() => coordinator.Render());

        StringAssert.Contains(error.Message, "state writes during render are not allowed");
        CollectionAssert.AreEqual(
            new[] { "root render", "component render" },
            error.Frames.Select(frame => frame.Stage).ToArray());
        Assert.IsInstanceOfType<InvalidOperationException>(error.InnerException);
    }

    private sealed class RenderPhaseStateWriteComponent : ComponentInstance
    {
        public RenderPhaseStateWriteComponent()
        {
            Count = new Csxaml.Runtime.State<int>(0, () => RequestRender?.Invoke());
        }

        private Csxaml.Runtime.State<int> Count { get; }

        public override Node Render()
        {
            Count.Value = Count.Value;
            return new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", Count.Value.ToString())],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>());
        }
    }
}
