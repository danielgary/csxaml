using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ScrollViewerAttachedPropertyApplicatorTests
{
    [TestMethod]
    public void Render_ScrollViewerAttachedProperties_ApplyAndClear()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstStack = (StackPanel)renderer.RenderProjectedRoot(
                CreateStack(
                [
                    new NativeAttachedPropertyValue(
                        "ScrollViewer",
                        "VerticalScrollMode",
                        ScrollMode.Disabled,
                        ValueKindHint.Enum),
                    new NativeAttachedPropertyValue(
                        "ScrollViewer",
                        "VerticalScrollBarVisibility",
                        ScrollBarVisibility.Hidden,
                        ValueKindHint.Enum)
                ]));
            var firstText = (TextBlock)firstStack.Children[0];

            Assert.AreEqual(ScrollMode.Disabled, firstText.GetValue(ScrollViewer.VerticalScrollModeProperty));
            Assert.AreEqual(
                ScrollBarVisibility.Hidden,
                firstText.GetValue(ScrollViewer.VerticalScrollBarVisibilityProperty));

            var secondStack = (StackPanel)renderer.RenderProjectedRoot(CreateStack([]));
            var secondText = (TextBlock)secondStack.Children[0];

            Assert.AreSame(firstStack, secondStack);
            Assert.AreSame(firstText, secondText);
            Assert.AreEqual(
                DependencyProperty.UnsetValue,
                secondText.ReadLocalValue(ScrollViewer.VerticalScrollModeProperty));
            Assert.AreEqual(
                DependencyProperty.UnsetValue,
                secondText.ReadLocalValue(ScrollViewer.VerticalScrollBarVisibilityProperty));
        });
    }

    [TestMethod]
    public void Render_AttachedProperties_DoNotClearUnownedLocalValues()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstStack = (StackPanel)renderer.RenderProjectedRoot(CreateStack([]));
            var firstText = (TextBlock)firstStack.Children[0];
            AutomationProperties.SetName(firstText, "External name");

            var secondStack = (StackPanel)renderer.RenderProjectedRoot(CreateStack([]));
            var secondText = (TextBlock)secondStack.Children[0];

            Assert.AreSame(firstStack, secondStack);
            Assert.AreSame(firstText, secondText);
            Assert.AreEqual("External name", AutomationProperties.GetName(secondText));
        });
    }

    private static NativeElementNode CreateStack(
        IReadOnlyList<NativeAttachedPropertyValue> attachedProperties)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    "total",
                    [new NativePropertyValue("Text", "42", ValueKindHint.String)],
                    attachedProperties,
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }
}
