using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class TextBlockControlAdapterTests
{
    [TestMethod]
    public void Render_TextBlock_AppliesAndClearsTextWrapping()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstText = (TextBlock)renderer.RenderProjectedRoot(
                CreateTextBlock(TextWrapping.Wrap));

            Assert.AreEqual(TextWrapping.Wrap, firstText.TextWrapping);

            var secondText = (TextBlock)renderer.RenderProjectedRoot(
                CreateTextBlock(null));

            Assert.AreSame(firstText, secondText);
            Assert.AreEqual(TextWrapping.NoWrap, secondText.TextWrapping);
        });
    }

    private static NativeElementNode CreateTextBlock(TextWrapping? textWrapping)
    {
        var properties = new List<NativePropertyValue>
        {
            new("Text", "Readable wrapping text", ValueKindHint.String)
        };
        if (textWrapping is not null)
        {
            properties.Add(new NativePropertyValue("TextWrapping", textWrapping, ValueKindHint.Enum));
        }

        return new NativeElementNode(
            "TextBlock",
            null,
            properties,
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
