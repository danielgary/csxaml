namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ControlledTextInputStateTests
{
    [TestMethod]
    public void Apply_DoesNotAssignWhenValueMatches()
    {
        var state = new ControlledTextInputState();
        var assignments = new List<string>();

        state.Apply("Draft", "Draft", value => assignments.Add(value));

        CollectionAssert.AreEqual(Array.Empty<string>(), assignments);
    }

    [TestMethod]
    public void Apply_UpdatesValueAndDispatch_IgnoresInternalWrite()
    {
        var state = new ControlledTextInputState();
        var assignments = new List<string>();
        var invocations = new List<string>();

        state.Apply(
            "Draft",
            "Updated",
            value =>
            {
                assignments.Add(value);
                state.Dispatch(value, nextValue => invocations.Add(nextValue));
            });

        CollectionAssert.AreEqual(new[] { "Updated" }, assignments);
        CollectionAssert.AreEqual(Array.Empty<string>(), invocations);
    }

    [TestMethod]
    public void Dispatch_ForwardsLatestValueWhenNotApplying()
    {
        var state = new ControlledTextInputState();
        var invocations = new List<string>();

        state.Dispatch("Updated", value => invocations.Add(value));

        CollectionAssert.AreEqual(new[] { "Updated" }, invocations);
    }

    [TestMethod]
    public void InputDeferral_HoldsRenderUntilTextInputCompletes()
    {
        var root = new FixedParentComponent();
        var coordinator = new ComponentTreeCoordinator(root);
        var updatedTrees = new List<NativeNode>();
        var state = new ControlledTextInputState();
        coordinator.TreeUpdated += updatedTrees.Add;
        coordinator.Render();

        try
        {
            state.BeginInput();
            root.Version.Value = 1;
            root.Version.Value = 2;

            Assert.HasCount(1, updatedTrees);
        }
        finally
        {
            state.CompleteInput();
        }

        Assert.HasCount(2, updatedTrees);
    }

    [TestMethod]
    public void ScheduleInputCompletion_ReleasesWithoutDispatcher()
    {
        var root = new FixedParentComponent();
        var coordinator = new ComponentTreeCoordinator(root);
        var updatedTrees = new List<NativeNode>();
        var state = new ControlledTextInputState();
        coordinator.TreeUpdated += updatedTrees.Add;
        coordinator.Render();

        state.BeginInput();
        root.Version.Value = 1;
        state.ScheduleInputCompletion(dispatcher: null);

        Assert.HasCount(2, updatedTrees);
    }

    [TestMethod]
    public void ConsumeFocusRestoreRequest_ReturnsPendingInputFocusOnce()
    {
        var state = new ControlledTextInputState();

        state.BeginInput(restoreFocusAfterInput: true);

        Assert.IsTrue(state.ConsumeFocusRestoreRequest());
        Assert.IsFalse(state.ConsumeFocusRestoreRequest());
    }
}
