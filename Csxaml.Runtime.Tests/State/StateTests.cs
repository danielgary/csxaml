namespace Csxaml.Runtime.Tests.State;

[TestClass]
public sealed class StateTests
{
    [TestMethod]
    public void SettingSameValue_InvalidatesOnce()
    {
        var invalidations = 0;
        var state = new Csxaml.Runtime.State<int>(1, () => invalidations++);

        state.Value = 1;

        Assert.AreEqual(1, invalidations);
    }

    [TestMethod]
    public void SettingDifferentValue_InvalidatesOnce()
    {
        var invalidations = 0;
        var state = new Csxaml.Runtime.State<int>(1, () => invalidations++);

        state.Value = 2;

        Assert.AreEqual(1, invalidations);
    }

    [TestMethod]
    public void SettingSameReference_InvalidatesOnce()
    {
        var invalidations = 0;
        var items = new List<int> { 1 };
        var state = new Csxaml.Runtime.State<List<int>>(items, () => invalidations++);

        state.Value = items;

        Assert.AreEqual(1, invalidations);
    }
}
