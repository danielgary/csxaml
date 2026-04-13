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
}
