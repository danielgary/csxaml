namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
public sealed class WheelScrollOffsetCalculatorTests
{
    [TestMethod]
    public void CalculateNextOffset_WheelDown_IncreasesOffset()
    {
        var next = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset: 10,
            scrollableHeight: 500,
            wheelDelta: -120);

        Assert.AreEqual(58, next);
    }

    [TestMethod]
    public void CalculateNextOffset_WheelUp_DecreasesOffset()
    {
        var next = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset: 100,
            scrollableHeight: 500,
            wheelDelta: 120);

        Assert.AreEqual(52, next);
    }

    [TestMethod]
    public void CalculateNextOffset_ClampsToBounds()
    {
        var beforeStart = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset: 10,
            scrollableHeight: 500,
            wheelDelta: 120);
        var afterEnd = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset: 490,
            scrollableHeight: 500,
            wheelDelta: -120);

        Assert.AreEqual(0, beforeStart);
        Assert.AreEqual(500, afterEnd);
    }
}
