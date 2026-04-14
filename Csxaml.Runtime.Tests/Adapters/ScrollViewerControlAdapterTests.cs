using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ScrollViewerControlAdapterTests
{
    [TestMethod]
    public void Render_ScrollViewer_RetainsSingleProjectedChild()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstScrollViewer = (ScrollViewer)renderer.RenderProjectedRoot(CreateScrollViewer("One"));
            var firstBorder = (Border)firstScrollViewer.Content;
            var firstText = (TextBlock)firstBorder.Child;

            var secondScrollViewer = (ScrollViewer)renderer.RenderProjectedRoot(CreateScrollViewer("Two"));
            var secondBorder = (Border)secondScrollViewer.Content;
            var secondText = (TextBlock)secondBorder.Child;

            Assert.AreSame(firstScrollViewer, secondScrollViewer);
            Assert.AreSame(firstBorder, secondBorder);
            Assert.AreSame(firstText, secondText);
            Assert.AreEqual("Two", secondText.Text);
        });
    }

    private static NativeElementNode CreateScrollViewer(string text)
    {
        return new NativeElementNode(
            "ScrollViewer",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "Border",
                    "editor-shell",
                    Array.Empty<NativePropertyValue>(),
                    Array.Empty<NativeEventValue>(),
                    [
                        new NativeElementNode(
                            "TextBlock",
                            "editor-text",
                            [new NativePropertyValue("Text", text, ValueKindHint.String)],
                            Array.Empty<NativeEventValue>(),
                            Array.Empty<Node>())
                    ])
            ]);
    }
}
