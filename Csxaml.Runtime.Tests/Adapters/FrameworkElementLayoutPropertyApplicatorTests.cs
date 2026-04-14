using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class FrameworkElementLayoutPropertyApplicatorTests
{
    [TestMethod]
    public void Render_Border_AppliesCommonLayoutPropsAndThicknessShorthand()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var border = (Border)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "Border",
                    null,
                    [
                        new NativePropertyValue("Margin", 12, ValueKindHint.Thickness),
                        new NativePropertyValue("Padding", 16, ValueKindHint.Thickness),
                        new NativePropertyValue("Width", 320, ValueKindHint.Double),
                        new NativePropertyValue("Height", 180, ValueKindHint.Double),
                        new NativePropertyValue("HorizontalAlignment", HorizontalAlignment.Center, ValueKindHint.Enum),
                        new NativePropertyValue("VerticalAlignment", VerticalAlignment.Bottom, ValueKindHint.Enum)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));

            Assert.AreEqual(new Thickness(12), border.Margin);
            Assert.AreEqual(new Thickness(16), border.Padding);
            Assert.AreEqual(320d, border.Width);
            Assert.AreEqual(180d, border.Height);
            Assert.AreEqual(HorizontalAlignment.Center, border.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, border.VerticalAlignment);
        });
    }
}
