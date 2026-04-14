using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class AttachedPropertyApplicatorTests
{
    [TestMethod]
    public void Render_AttachedProperties_ApplyToProjectedControls()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstGrid = (Grid)renderer.RenderProjectedRoot(CreateGrid(1, 2, "First Editor", "SelectedTodoTitle"));
            var firstTextBlock = (TextBlock)firstGrid.Children[0];

            var secondGrid = (Grid)renderer.RenderProjectedRoot(CreateGrid(2, 3, "Second Editor", "SelectedTodoTitle"));
            var secondTextBlock = (TextBlock)secondGrid.Children[0];

            Assert.AreSame(firstGrid, secondGrid);
            Assert.AreSame(firstTextBlock, secondTextBlock);
            Assert.AreEqual(2, Grid.GetRow(secondTextBlock));
            Assert.AreEqual(3, Grid.GetColumnSpan(secondTextBlock));
            Assert.AreEqual("Second Editor", AutomationProperties.GetName(secondTextBlock));
            Assert.AreEqual("SelectedTodoTitle", AutomationProperties.GetAutomationId(secondTextBlock));
        });
    }

    private static NativeElementNode CreateGrid(
        int row,
        int columnSpan,
        string automationName,
        string automationId)
    {
        return new NativeElementNode(
            "Grid",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    "editor-title",
                    [new NativePropertyValue("Text", "Title", ValueKindHint.String)],
                    [
                        new NativeAttachedPropertyValue("Grid", "Row", row, ValueKindHint.Int),
                        new NativeAttachedPropertyValue("Grid", "ColumnSpan", columnSpan, ValueKindHint.Int),
                        new NativeAttachedPropertyValue("AutomationProperties", "Name", automationName, ValueKindHint.String),
                        new NativeAttachedPropertyValue("AutomationProperties", "AutomationId", automationId, ValueKindHint.String)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }
}
