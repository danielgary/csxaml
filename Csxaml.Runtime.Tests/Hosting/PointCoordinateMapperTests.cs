namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
public sealed class PointCoordinateMapperTests
{
    [TestMethod]
    public void PhysicalPixelsToEffectivePixels_UsesWindowDpiScale()
    {
        var point = PointCoordinateMapper.PhysicalPixelsToEffectivePixels(
            x: 300,
            y: 450,
            dpi: 144);

        Assert.AreEqual(200, point.X);
        Assert.AreEqual(300, point.Y);
    }

    [TestMethod]
    public void PhysicalPixelsToEffectivePixels_KeepsNinetySixDpiCoordinates()
    {
        var point = PointCoordinateMapper.PhysicalPixelsToEffectivePixels(
            x: 300,
            y: 450,
            dpi: 96);

        Assert.AreEqual(300, point.X);
        Assert.AreEqual(450, point.Y);
    }
}
