using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator.Tests.Filtering;

[TestClass]
public sealed class SupportedControlFilterTests
{
    [TestMethod]
    public void BuildMetadata_Button_UsesCuratedSubsetAndEventMapping()
    {
        var definition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(Button));
        var discovered = new ControlMetadataDiscoverer().Discover(typeof(Button));

        var metadata = new SupportedControlFilter().BuildMetadata(definition, discovered);

        CollectionAssert.AreEquivalent(
            new[]
            {
                "Background",
                "Content",
                "FontSize",
                "Foreground",
                "Height",
                "HorizontalAlignment",
                "Margin",
                "Style",
                "VerticalAlignment",
                "Width"
            },
            metadata.Properties.Select(property => property.Name).ToArray());
        Assert.AreEqual(
            ParseStyleHint(),
            metadata.Properties.Single(property => property.Name == "Style").ValueKindHint);
        Assert.AreEqual("OnClick", metadata.Events.Single().ExposedName);
        Assert.AreEqual("System.Action", metadata.Events.Single().HandlerTypeName);
        Assert.AreEqual(EventBindingKind.Direct, metadata.Events.Single().BindingKind);
        Assert.AreEqual(ControlChildKind.None, metadata.ChildKind);
    }

    [TestMethod]
    public void BuildMetadata_TextInputControls_ExposeProjectedEvents()
    {
        var textBoxDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(TextBox));
        var textBoxDiscovered = new ControlMetadataDiscoverer().Discover(typeof(TextBox));
        var textBoxMetadata = new SupportedControlFilter().BuildMetadata(textBoxDefinition, textBoxDiscovered);

        CollectionAssert.Contains(textBoxMetadata.Properties.Select(property => property.Name).ToList(), "Text");
        Assert.AreEqual("OnTextChanged", textBoxMetadata.Events.Single().ExposedName);
        Assert.AreEqual("System.Action<string>", textBoxMetadata.Events.Single().HandlerTypeName);
        Assert.AreEqual(ValueKindHint.String, textBoxMetadata.Events.Single().ValueKindHint);
        Assert.AreEqual(EventBindingKind.TextValueChanged, textBoxMetadata.Events.Single().BindingKind);

        var checkBoxDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(CheckBox));
        var checkBoxDiscovered = new ControlMetadataDiscoverer().Discover(typeof(CheckBox));
        var checkBoxMetadata = new SupportedControlFilter().BuildMetadata(checkBoxDefinition, checkBoxDiscovered);

        CollectionAssert.Contains(checkBoxMetadata.Properties.Select(property => property.Name).ToList(), "IsChecked");
        Assert.AreEqual(ValueKindHint.Bool, checkBoxMetadata.Properties.Single(property => property.Name == "IsChecked").ValueKindHint);
        Assert.AreEqual("OnCheckedChanged", checkBoxMetadata.Events.Single().ExposedName);
        Assert.AreEqual("System.Action<bool>", checkBoxMetadata.Events.Single().HandlerTypeName);
        Assert.AreEqual(ValueKindHint.Bool, checkBoxMetadata.Events.Single().ValueKindHint);
        Assert.AreEqual(EventBindingKind.BoolValueChanged, checkBoxMetadata.Events.Single().BindingKind);
    }

    [TestMethod]
    public void GeneratedRegistry_ContainsExpectedCuratedControls()
    {
        var names = ControlMetadataRegistry.Controls
            .Select(control => control.TagName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { "Border", "Button", "CheckBox", "Grid", "ScrollViewer", "StackPanel", "TextBlock", "TextBox" },
            names);
    }

    [TestMethod]
    public void GeneratedRegistry_ExposesCommonLayoutPropertiesOnSupportedControls()
    {
        var border = ControlMetadataRegistry.GetControl("Border");
        var grid = ControlMetadataRegistry.GetControl("Grid");
        var scrollViewer = ControlMetadataRegistry.GetControl("ScrollViewer");

        CollectionAssert.IsSubsetOf(
            new[] { "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            border.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "ColumnDefinitions", "RowDefinitions", "Margin", "Width" },
            grid.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            scrollViewer.Properties.Select(property => property.Name).ToList());
    }

    [TestMethod]
    public void AttachedPropertyRegistry_ContainsInitialGridAndAutomationSlice()
    {
        var names = AttachedPropertyMetadataRegistry.Properties
            .Select(property => property.QualifiedName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                "AutomationProperties.AutomationId",
                "AutomationProperties.Name",
                "Grid.Column",
                "Grid.ColumnSpan",
                "Grid.Row",
                "Grid.RowSpan"
            },
            names);

        Assert.AreEqual(
            "Grid",
            AttachedPropertyMetadataRegistry.GetProperty("Grid", "Row").RequiredParentTagName);
        Assert.AreEqual(
            ValueKindHint.String,
            AttachedPropertyMetadataRegistry.GetProperty("AutomationProperties", "Name").ValueKindHint);
    }

    private static ValueKindHint ParseStyleHint()
    {
        return Enum.Parse<ValueKindHint>("Style");
    }
}
