using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ExpandedAttachedPropertyApplicatorTests
{
    [TestMethod]
    public void Render_CanvasAttachedProperties_ApplyAndClear()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstCanvas = (Canvas)renderer.RenderProjectedRoot(
                CreateCanvas(
                [
                    new NativeAttachedPropertyValue("Canvas", "Left", 20.5, ValueKindHint.Double),
                    new NativeAttachedPropertyValue("Canvas", "Top", 12.0, ValueKindHint.Double),
                    new NativeAttachedPropertyValue("Canvas", "ZIndex", 3, ValueKindHint.Int)
                ]));
            var firstButton = (Button)firstCanvas.Children[0];

            var secondCanvas = (Canvas)renderer.RenderProjectedRoot(CreateCanvas([]));
            var secondButton = (Button)secondCanvas.Children[0];

            Assert.AreSame(firstCanvas, secondCanvas);
            Assert.AreSame(firstButton, secondButton);
            Assert.IsTrue(double.IsNaN(Canvas.GetLeft(secondButton)));
            Assert.IsTrue(double.IsNaN(Canvas.GetTop(secondButton)));
            Assert.AreEqual(0, Canvas.GetZIndex(secondButton));
        });
    }

    [TestMethod]
    public void Render_RelativePanelAttachedProperties_ApplyAndClear()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstPanel = (RelativePanel)renderer.RenderProjectedRoot(
                CreateRelativePanel(includeAttached: true));
            var firstAnchor = (TextBlock)firstPanel.Children[0];
            var firstFollower = (TextBlock)firstPanel.Children[1];

            Assert.IsTrue(RelativePanel.GetAlignLeftWithPanel(firstAnchor));
            Assert.IsTrue(RelativePanel.GetAlignTopWithPanel(firstAnchor));
            Assert.AreEqual("Anchor", RelativePanel.GetRightOf(firstFollower));
            Assert.AreEqual("Anchor", RelativePanel.GetBelow(firstFollower));

            var secondPanel = (RelativePanel)renderer.RenderProjectedRoot(
                CreateRelativePanel(includeAttached: false));
            var secondAnchor = (TextBlock)secondPanel.Children[0];
            var secondFollower = (TextBlock)secondPanel.Children[1];

            Assert.AreSame(firstPanel, secondPanel);
            Assert.AreSame(firstAnchor, secondAnchor);
            Assert.AreSame(firstFollower, secondFollower);
            Assert.IsFalse(RelativePanel.GetAlignLeftWithPanel(secondAnchor));
            Assert.IsFalse(RelativePanel.GetAlignTopWithPanel(secondAnchor));
            Assert.IsNull(RelativePanel.GetRightOf(secondFollower));
            Assert.IsNull(RelativePanel.GetBelow(secondFollower));
        });
    }

    [TestMethod]
    public void Render_VariableSizedWrapGridAttachedProperties_ApplyAndClear()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();

            var firstGrid = (VariableSizedWrapGrid)renderer.RenderProjectedRoot(
                CreateVariableSizedWrapGrid(includeAttached: true));
            var firstChild = (Border)firstGrid.Children[0];

            Assert.AreEqual(2, VariableSizedWrapGrid.GetColumnSpan(firstChild));
            Assert.AreEqual(3, VariableSizedWrapGrid.GetRowSpan(firstChild));

            var secondGrid = (VariableSizedWrapGrid)renderer.RenderProjectedRoot(
                CreateVariableSizedWrapGrid(includeAttached: false));
            var secondChild = (Border)secondGrid.Children[0];

            Assert.AreSame(firstGrid, secondGrid);
            Assert.AreSame(firstChild, secondChild);
            Assert.AreEqual(1, VariableSizedWrapGrid.GetColumnSpan(secondChild));
            Assert.AreEqual(1, VariableSizedWrapGrid.GetRowSpan(secondChild));
        });
    }

    [TestMethod]
    public void Render_AutomationAndToolTipAttachedProperties_ApplyAndClear()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();
            var label = new TextBlock();

            var firstStack = (StackPanel)renderer.RenderProjectedRoot(
                CreateAutomationStack(
                [
                    new NativeAttachedPropertyValue("AutomationProperties", "HelpText", "Total help", ValueKindHint.String),
                    new NativeAttachedPropertyValue("AutomationProperties", "ItemStatus", "Ready", ValueKindHint.String),
                    new NativeAttachedPropertyValue("AutomationProperties", "ItemType", "Summary", ValueKindHint.String),
                    new NativeAttachedPropertyValue("AutomationProperties", "LabeledBy", label, ValueKindHint.Object),
                    new NativeAttachedPropertyValue("ToolTipService", "ToolTip", "Saved", ValueKindHint.String)
                ]));
            var firstText = (TextBlock)firstStack.Children[0];

            Assert.AreEqual("Total help", AutomationProperties.GetHelpText(firstText));
            Assert.AreEqual("Ready", AutomationProperties.GetItemStatus(firstText));
            Assert.AreEqual("Summary", AutomationProperties.GetItemType(firstText));
            Assert.AreSame(label, AutomationProperties.GetLabeledBy(firstText));
            Assert.AreEqual("Saved", ToolTipService.GetToolTip(firstText));

            var secondStack = (StackPanel)renderer.RenderProjectedRoot(CreateAutomationStack([]));
            var secondText = (TextBlock)secondStack.Children[0];

            Assert.AreSame(firstStack, secondStack);
            Assert.AreSame(firstText, secondText);
            Assert.AreEqual(string.Empty, AutomationProperties.GetHelpText(secondText));
            Assert.AreEqual(string.Empty, AutomationProperties.GetItemStatus(secondText));
            Assert.AreEqual(string.Empty, AutomationProperties.GetItemType(secondText));
            Assert.IsNull(AutomationProperties.GetLabeledBy(secondText));
            Assert.IsNull(ToolTipService.GetToolTip(secondText));
        });
    }

    private static NativeElementNode CreateCanvas(
        IReadOnlyList<NativeAttachedPropertyValue> attachedProperties)
    {
        return new NativeElementNode(
            "Canvas",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "Button",
                    "move-button",
                    [new NativePropertyValue("Content", "Move", ValueKindHint.String)],
                    attachedProperties,
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }

    private static NativeElementNode CreateRelativePanel(bool includeAttached)
    {
        IReadOnlyList<NativeAttachedPropertyValue> anchorProperties = includeAttached
            ?
            [
                new NativeAttachedPropertyValue("RelativePanel", "AlignLeftWithPanel", true, ValueKindHint.Bool),
                new NativeAttachedPropertyValue("RelativePanel", "AlignTopWithPanel", true, ValueKindHint.Bool)
            ]
            : Array.Empty<NativeAttachedPropertyValue>();
        IReadOnlyList<NativeAttachedPropertyValue> followerProperties = includeAttached
            ?
            [
                new NativeAttachedPropertyValue("RelativePanel", "RightOf", "Anchor", ValueKindHint.Object),
                new NativeAttachedPropertyValue("RelativePanel", "Below", "Anchor", ValueKindHint.Object)
            ]
            : Array.Empty<NativeAttachedPropertyValue>();

        return new NativeElementNode(
            "RelativePanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    "anchor",
                    [new NativePropertyValue("Text", "Anchor", ValueKindHint.String)],
                    anchorProperties,
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()),
                new NativeElementNode(
                    "TextBlock",
                    "follower",
                    [new NativePropertyValue("Text", "Follower", ValueKindHint.String)],
                    followerProperties,
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }

    private static NativeElementNode CreateVariableSizedWrapGrid(bool includeAttached)
    {
        IReadOnlyList<NativeAttachedPropertyValue> attachedProperties = includeAttached
            ?
            [
                new NativeAttachedPropertyValue("VariableSizedWrapGrid", "ColumnSpan", 2, ValueKindHint.Int),
                new NativeAttachedPropertyValue("VariableSizedWrapGrid", "RowSpan", 3, ValueKindHint.Int)
            ]
            : Array.Empty<NativeAttachedPropertyValue>();

        return new NativeElementNode(
            "VariableSizedWrapGrid",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "Border",
                    "tile",
                    Array.Empty<NativePropertyValue>(),
                    attachedProperties,
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }

    private static NativeElementNode CreateAutomationStack(
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
