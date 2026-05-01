using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ListViewControlAdapterTests
{
    [TestMethod]
    public void Render_ListView_RetainsInternalScrollerForStableItemsSource()
    {
        WinUiTestEnvironment.RunInWindow(window =>
        {
            var items = Enumerable.Range(1, 30).Select(index => $"Item {index}").ToArray();
            var renderer = new WinUiNodeRenderer();

            var firstRoot = (StackPanel)renderer.RenderProjectedRoot(CreateHostNode(items, "Ready"));
            window.Content = firstRoot;
            window.Activate();
            firstRoot.UpdateLayout();

            var firstList = (ListView)firstRoot.Children[0];
            var firstScroller = FindDescendant<ScrollViewer>(firstList);
            if (firstScroller is null || firstScroller.ScrollableHeight <= 0)
            {
                Assert.Inconclusive("ListView did not create a scrollable template in this test environment.");
            }

            firstScroller.ChangeView(null, 40, null, disableAnimation: true);
            firstRoot.UpdateLayout();

            var secondRoot = (StackPanel)renderer.RenderProjectedRoot(CreateHostNode(items, "Updated"));
            secondRoot.UpdateLayout();
            var secondList = (ListView)secondRoot.Children[0];
            var secondScroller = FindDescendant<ScrollViewer>(secondList);

            Assert.AreSame(firstRoot, secondRoot);
            Assert.AreSame(firstList, secondList);
            Assert.AreSame(firstScroller, secondScroller);
            Assert.IsGreaterThan(secondScroller!.VerticalOffset, 0);
        });
    }

    private static NativeElementNode CreateHostNode(object itemsSource, string status)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "ListView",
                    null,
                    [
                        new NativePropertyValue("Height", 120, ValueKindHint.Double),
                        new NativePropertyValue("ItemsSource", itemsSource, ValueKindHint.Object)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()),
                new NativeElementNode(
                    "TextBlock",
                    null,
                    [new NativePropertyValue("Text", status, ValueKindHint.String)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }

    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
