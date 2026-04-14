using System.Reflection;
using Csxaml.ExternalControls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ExternalControlAdapterTests
{
    [TestCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [TestMethod]
    public void Render_InfoBar_AppliesPackagePropertiesAndRetainsElement()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateInfoBarDescriptor());
            var renderer = new WinUiNodeRenderer();

            var firstInfoBar = (InfoBar)renderer.RenderProjectedRoot(
                CreateInfoBarNode(
                    isOpen: true,
                    message: "External package control",
                    severity: InfoBarSeverity.Informational,
                    title: "Interop"));
            var secondInfoBar = (InfoBar)renderer.RenderProjectedRoot(
                CreateInfoBarNode(
                    isOpen: false,
                    message: "Updated package control",
                    severity: InfoBarSeverity.Warning,
                    title: null));

            Assert.AreSame(firstInfoBar, secondInfoBar);
            Assert.IsFalse(secondInfoBar.IsOpen);
            Assert.AreEqual("Updated package control", secondInfoBar.Message);
            Assert.AreEqual(InfoBarSeverity.Warning, secondInfoBar.Severity);
            Assert.IsTrue(string.IsNullOrEmpty(secondInfoBar.Title));
        });
    }

    [TestMethod]
    public void Render_StatusButton_AppliesPropertyEventChildAndAttachedState()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateStatusButtonDescriptor());
            var renderer = new WinUiNodeRenderer();
            var clickCount = 0;

            var firstGrid = (Grid)renderer.RenderProjectedRoot(
                CreateStatusButtonGrid(
                    badgeText: "todo-2",
                    childText: "Wire runtime",
                    onClick: () => clickCount++));
            var firstButton = (StatusButton)firstGrid.Children[0];
            var firstChild = (TextBlock)firstButton.Content;

            Assert.AreEqual("todo-2", firstButton.BadgeText);
            Assert.AreEqual(1, Grid.GetRow(firstButton));
            InvokeClick(firstButton);
            Assert.AreEqual(1, clickCount);

            var secondGrid = (Grid)renderer.RenderProjectedRoot(
                CreateStatusButtonGrid(
                    badgeText: null,
                    childText: "Write tests",
                    onClick: null));
            var secondButton = (StatusButton)secondGrid.Children[0];
            var secondChild = (TextBlock)secondButton.Content;

            Assert.AreSame(firstGrid, secondGrid);
            Assert.AreSame(firstButton, secondButton);
            Assert.AreSame(firstChild, secondChild);
            Assert.AreEqual("Write tests", secondChild.Text);
            Assert.AreEqual(string.Empty, secondButton.BadgeText ?? string.Empty);
            InvokeClick(secondButton);
            Assert.AreEqual(1, clickCount);
        });
    }

    private static NativeElementNode CreateInfoBarNode(
        bool isOpen,
        string message,
        InfoBarSeverity severity,
        string? title)
    {
        var properties = new List<NativePropertyValue>
        {
            new("IsOpen", isOpen, ValueKindHint.Bool),
            new("Message", message, ValueKindHint.String),
            new("Severity", severity, ValueKindHint.Enum)
        };
        if (title is not null)
        {
            properties.Add(new NativePropertyValue("Title", title, ValueKindHint.String));
        }

        return new NativeElementNode(
            typeof(InfoBar).FullName!,
            null,
            properties,
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static ExternalControlDescriptor CreateInfoBarDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(InfoBar),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(InfoBar).FullName!,
                typeof(InfoBar).FullName!,
                typeof(Control).FullName,
                ControlChildKind.Single,
                [
                    new PropertyMetadata("IsOpen", typeof(bool).FullName!, true, true, false, true, ValueKindHint.Bool),
                    new PropertyMetadata("Message", typeof(string).FullName!, true, false, false, true, ValueKindHint.String),
                    new PropertyMetadata("Severity", typeof(InfoBarSeverity).FullName!, true, false, false, true, ValueKindHint.Enum),
                    new PropertyMetadata("Title", typeof(string).FullName!, true, false, false, true, ValueKindHint.String)
                ],
                Array.Empty<EventMetadata>()));
    }

    private static ExternalControlDescriptor CreateStatusButtonDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(StatusButton),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(StatusButton).FullName!,
                typeof(StatusButton).FullName!,
                typeof(Button).FullName,
                ControlChildKind.Single,
                [
                    new PropertyMetadata("BadgeText", typeof(string).FullName!, true, true, false, true, ValueKindHint.String)
                ],
                [
                    new EventMetadata("Click", "OnClick", typeof(Action).FullName!, true, ValueKindHint.Unknown, EventBindingKind.Direct)
                ]));
    }

    private static NativeElementNode CreateStatusButtonGrid(
        string? badgeText,
        string childText,
        Action? onClick)
    {
        var properties = new List<NativePropertyValue>();
        if (badgeText is not null)
        {
            properties.Add(new NativePropertyValue("BadgeText", badgeText, ValueKindHint.String));
        }

        var events = onClick is null
            ? Array.Empty<NativeEventValue>()
            : [new NativeEventValue("OnClick", onClick, ValueKindHint.Unknown)];
        return new NativeElementNode(
            "Grid",
            null,
            [new NativePropertyValue("RowDefinitions", "Auto,*", ValueKindHint.Object)],
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    typeof(StatusButton).FullName!,
                    null,
                    properties,
                    [new NativeAttachedPropertyValue("Grid", "Row", 1, ValueKindHint.Int)],
                    events,
                    [
                        new NativeElementNode(
                            "TextBlock",
                            null,
                            [new NativePropertyValue("Text", childText, ValueKindHint.String)],
                            Array.Empty<NativeEventValue>(),
                            Array.Empty<Node>())
                    ])
            ]);
    }

    private static void InvokeClick(StatusButton button)
    {
        typeof(ButtonBase)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, null);
    }
}
