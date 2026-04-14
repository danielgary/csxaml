using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class FrameworkElementStylePropertyApplicatorTests
{
    [TestMethod]
    public void Render_Button_AppliesAndClearsStyle()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();
            var style = CreateButtonStyle(255, 0, 0);

            var firstButton = (Button)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "Button",
                    null,
                    [
                        new NativePropertyValue("Content", "Save", ValueKindHint.Object),
                        new NativePropertyValue("Style", style, ValueKindHint.Unknown)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));
            var secondButton = (Button)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "Button",
                    null,
                    [new NativePropertyValue("Content", "Save", ValueKindHint.Object)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));

            Assert.AreSame(firstButton, secondButton);
            Assert.AreSame(style, firstButton.Style);
            Assert.IsNull(secondButton.Style);
        });
    }

    [TestMethod]
    public void Render_Button_ExplicitForegroundOverridesStyleSetter()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();
            var style = CreateButtonStyle(255, 0, 0);
            var button = (Button)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "Button",
                    null,
                    [
                        new NativePropertyValue("Content", "Save", ValueKindHint.Object),
                        new NativePropertyValue("Style", style, ValueKindHint.Unknown),
                        new NativePropertyValue("Foreground", new ArgbColor(255, 0, 128, 0), ValueKindHint.Brush)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));

            Assert.AreSame(style, button.Style);
            Assert.IsInstanceOfType<SolidColorBrush>(button.Foreground);
            var foreground = (SolidColorBrush)button.Foreground;
            Assert.AreEqual(ColorHelper.FromArgb(255, 0, 128, 0), foreground.Color);
        });
    }

    private static Style CreateButtonStyle(byte r, byte g, byte b)
    {
        var style = new Style { TargetType = typeof(Button) };
        style.Setters.Add(
            new Setter(
                Control.ForegroundProperty,
                new SolidColorBrush(ColorHelper.FromArgb(255, r, g, b))));
        return style;
    }
}
