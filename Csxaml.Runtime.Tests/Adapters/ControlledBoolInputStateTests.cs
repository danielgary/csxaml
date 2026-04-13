namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ControlledBoolInputStateTests
{
    [TestMethod]
    public void Apply_DoesNotAssignWhenValueMatches()
    {
        var state = new ControlledBoolInputState();
        var assignments = new List<bool>();

        state.Apply(false, false, value => assignments.Add(value));

        CollectionAssert.AreEqual(Array.Empty<bool>(), assignments);
    }

    [TestMethod]
    public void Apply_UpdatesValueAndDispatch_IgnoresInternalWrite()
    {
        var state = new ControlledBoolInputState();
        var assignments = new List<bool>();
        var invocations = new List<bool>();

        state.Apply(
            false,
            true,
            value =>
            {
                assignments.Add(value);
                state.Dispatch(value, nextValue => invocations.Add(nextValue));
            });

        CollectionAssert.AreEqual(new[] { true }, assignments);
        CollectionAssert.AreEqual(Array.Empty<bool>(), invocations);
    }

    [TestMethod]
    public void Dispatch_ForwardsLatestValueWhenNotApplying()
    {
        var state = new ControlledBoolInputState();
        var invocations = new List<bool>();

        state.Dispatch(true, value => invocations.Add(value));
        state.Dispatch(false, value => invocations.Add(value));

        CollectionAssert.AreEqual(new[] { true, false }, invocations);
    }
}
