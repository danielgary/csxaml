namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class TextSelectionRangeTests
{
    [TestMethod]
    public void Clamp_PreservesRange_WhenTextStillFits()
    {
        var range = new TextSelectionRange(2, 3);

        var clamped = range.Clamp(10);

        Assert.AreEqual(new TextSelectionRange(2, 3), clamped);
    }

    [TestMethod]
    public void Clamp_ShrinksSelection_WhenTextBecomesShorter()
    {
        var range = new TextSelectionRange(6, 4);

        var clamped = range.Clamp(7);

        Assert.AreEqual(new TextSelectionRange(6, 1), clamped);
    }

    [TestMethod]
    public void Clamp_BringsNegativeValuesBackIntoBounds()
    {
        var range = new TextSelectionRange(-3, -1);

        var clamped = range.Clamp(5);

        Assert.AreEqual(new TextSelectionRange(0, 0), clamped);
    }
}
