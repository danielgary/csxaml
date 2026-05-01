namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
public sealed class MouseWheelMessageTests
{
    [TestMethod]
    public void FromNative_ReadsDeltaAndScreenCoordinates()
    {
        var message = MouseWheelMessage.FromNative(
            (UIntPtr)0x00780000u,
            (IntPtr)0x00F00140);

        Assert.AreEqual(120, message.Delta);
        Assert.AreEqual(320, message.ScreenX);
        Assert.AreEqual(240, message.ScreenY);
    }

    [TestMethod]
    public void FromNative_ReadsNegativeDeltaAndCoordinates()
    {
        var lParam = unchecked((int)0xFFE2FFF4);

        var message = MouseWheelMessage.FromNative(
            (UIntPtr)0xFF880000u,
            (IntPtr)lParam);

        Assert.AreEqual(-120, message.Delta);
        Assert.AreEqual(-12, message.ScreenX);
        Assert.AreEqual(-30, message.ScreenY);
    }
}
