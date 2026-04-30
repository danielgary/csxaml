namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ControlledRangeInputStateTests
{
    [TestMethod]
    public void Apply_DispatchIgnoresInternalWrite()
    {
        var state = new ControlledRangeInputState();
        var invocations = new List<double>();

        state.Apply(() => state.Dispatch(42d, value => invocations.Add(value)));

        CollectionAssert.AreEqual(Array.Empty<double>(), invocations);
    }

    [TestMethod]
    public void Dispatch_ForwardsLatestValueWhenNotApplying()
    {
        var state = new ControlledRangeInputState();
        var invocations = new List<double>();

        state.Dispatch(24d, value => invocations.Add(value));

        CollectionAssert.AreEqual(new[] { 24d }, invocations);
    }
}
