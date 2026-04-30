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

    [TestMethod]
    public void Render_ScrollViewer_AppliesScrollModeProperties()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var scrollViewer = (ScrollViewer)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "ScrollViewer",
                    null,
                    [
                        new NativePropertyValue(
                            "HorizontalScrollBarVisibility",
                            ScrollBarVisibility.Disabled,
                            ValueKindHint.Enum),
                        new NativePropertyValue(
                            "HorizontalScrollMode",
                            ScrollMode.Disabled,
                            ValueKindHint.Enum),
                        new NativePropertyValue(
                            "VerticalScrollBarVisibility",
                            ScrollBarVisibility.Auto,
                            ValueKindHint.Enum),
                        new NativePropertyValue(
                            "VerticalScrollMode",
                            ScrollMode.Auto,
                            ValueKindHint.Enum)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));

            Assert.AreEqual(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
            Assert.AreEqual(ScrollMode.Disabled, scrollViewer.HorizontalScrollMode);
            Assert.AreEqual(ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);
            Assert.AreEqual(ScrollMode.Auto, scrollViewer.VerticalScrollMode);
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
