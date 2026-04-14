using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class GridControlAdapterTests
{
    [TestMethod]
    public void Render_Grid_ReordersRetainedChildrenAndAppliesDefinitions()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstGrid = (Grid)renderer.RenderProjectedRoot(CreateGrid(("a", 0), ("b", 1)));
            var firstA = (TextBlock)firstGrid.Children[0];
            var firstB = (TextBlock)firstGrid.Children[1];

            var secondGrid = (Grid)renderer.RenderProjectedRoot(CreateGrid(("b", 0), ("a", 1)));

            Assert.AreSame(firstGrid, secondGrid);
            Assert.AreSame(firstB, secondGrid.Children[0]);
            Assert.AreSame(firstA, secondGrid.Children[1]);
            Assert.AreEqual(2, secondGrid.ColumnDefinitions.Count);
            Assert.AreEqual(2, secondGrid.RowDefinitions.Count);
        });
    }

    private static NativeElementNode CreateGrid((string Key, int Column) first, (string Key, int Column) second)
    {
        return new NativeElementNode(
            "Grid",
            null,
            [
                new NativePropertyValue("RowDefinitions", "Auto,*", ValueKindHint.Object),
                new NativePropertyValue("ColumnDefinitions", "260,*", ValueKindHint.Object)
            ],
            Array.Empty<NativeEventValue>(),
            [
                CreateChild(first.Key, first.Column),
                CreateChild(second.Key, second.Column)
            ]);
    }

    private static NativeElementNode CreateChild(string key, int column)
    {
        return new NativeElementNode(
            "TextBlock",
            key,
            [new NativePropertyValue("Text", key, ValueKindHint.String)],
            [new NativeAttachedPropertyValue("Grid", "Column", column, ValueKindHint.Int)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
