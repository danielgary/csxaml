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
        var click = metadata.Events.Single(@event => @event.ExposedName == "OnClick");
        Assert.AreEqual("System.Action", click.HandlerTypeName);
        Assert.AreEqual(EventBindingKind.Direct, click.BindingKind);
        AssertCommonEventArgs(metadata);
        Assert.AreEqual(ControlChildKind.None, metadata.ChildKind);
        Assert.AreEqual(ControlContentKind.None, metadata.Content.Kind);
        Assert.AreEqual(ControlContentSource.BuiltInMetadata, metadata.Content.Source);
    }

    [TestMethod]
    public void BuildMetadata_TextInputControls_ExposeProjectedEvents()
    {
        var textBoxDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(TextBox));
        var textBoxDiscovered = new ControlMetadataDiscoverer().Discover(typeof(TextBox));
        var textBoxMetadata = new SupportedControlFilter().BuildMetadata(textBoxDefinition, textBoxDiscovered);

        CollectionAssert.Contains(textBoxMetadata.Properties.Select(property => property.Name).ToList(), "Text");
        var textChanged = textBoxMetadata.Events.Single(@event => @event.ExposedName == "OnTextChanged");
        Assert.AreEqual("System.Action<string>", textChanged.HandlerTypeName);
        Assert.AreEqual(ValueKindHint.String, textChanged.ValueKindHint);
        Assert.AreEqual(EventBindingKind.TextValueChanged, textChanged.BindingKind);

        var checkBoxDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(CheckBox));
        var checkBoxDiscovered = new ControlMetadataDiscoverer().Discover(typeof(CheckBox));
        var checkBoxMetadata = new SupportedControlFilter().BuildMetadata(checkBoxDefinition, checkBoxDiscovered);

        CollectionAssert.Contains(checkBoxMetadata.Properties.Select(property => property.Name).ToList(), "IsChecked");
        Assert.AreEqual(ValueKindHint.Bool, checkBoxMetadata.Properties.Single(property => property.Name == "IsChecked").ValueKindHint);
        var checkedChanged = checkBoxMetadata.Events.Single(@event => @event.ExposedName == "OnCheckedChanged");
        Assert.AreEqual("System.Action<bool>", checkedChanged.HandlerTypeName);
        Assert.AreEqual(ValueKindHint.Bool, checkedChanged.ValueKindHint);
        Assert.AreEqual(EventBindingKind.BoolValueChanged, checkedChanged.BindingKind);
    }

    [TestMethod]
    public void BuildMetadata_CommonWinUiEvents_ExposeTypedEventArgs()
    {
        var listView = ControlMetadataRegistry.GetControl("ListView");
        var slider = ControlMetadataRegistry.GetControl("Slider");
        var autoSuggestBox = ControlMetadataRegistry.GetControl("AutoSuggestBox");
        var frame = ControlMetadataRegistry.GetControl("Frame");
        var textBox = ControlMetadataRegistry.GetControl("TextBox");

        AssertEventArgs(
            listView,
            "OnSelectionChanged",
            "System.Action<Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs>");
        AssertEventArgs(
            listView,
            "OnItemClick",
            "System.Action<Microsoft.UI.Xaml.Controls.ItemClickEventArgs>");
        AssertEventArgs(
            slider,
            "OnValueChanged",
            "System.Action<Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs>");
        AssertEventArgs(
            autoSuggestBox,
            "OnQuerySubmitted",
            "System.Action<Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs>");
        AssertEventArgs(
            autoSuggestBox,
            "OnSuggestionChosen",
            "System.Action<Microsoft.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs>");
        AssertEventArgs(
            textBox,
            "OnKeyDown",
            "System.Action<Microsoft.UI.Xaml.Input.KeyRoutedEventArgs>");
        AssertEventArgs(
            textBox,
            "OnPointerPressed",
            "System.Action<Microsoft.UI.Xaml.Input.PointerRoutedEventArgs>");
        AssertEventArgs(
            frame,
            "OnNavigated",
            "System.Action<Microsoft.UI.Xaml.Navigation.NavigationEventArgs>");
    }

    [TestMethod]
    public void GeneratedRegistry_ContainsExpectedCuratedControls()
    {
        var names = ControlMetadataRegistry.Controls
            .Select(control => control.TagName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                "AutoSuggestBox",
                "Border",
                "Button",
                "Canvas",
                "CheckBox",
                "Frame",
                "Grid",
                "ListView",
                "RelativePanel",
                "ScrollViewer",
                "Slider",
                "StackPanel",
                "TextBlock",
                "TextBox",
                "VariableSizedWrapGrid"
            },
            names);
    }

    [TestMethod]
    public void GeneratedRegistry_ExposesCommonLayoutPropertiesOnSupportedControls()
    {
        var border = ControlMetadataRegistry.GetControl("Border");
        var canvas = ControlMetadataRegistry.GetControl("Canvas");
        var grid = ControlMetadataRegistry.GetControl("Grid");
        var relativePanel = ControlMetadataRegistry.GetControl("RelativePanel");
        var scrollViewer = ControlMetadataRegistry.GetControl("ScrollViewer");
        var wrapGrid = ControlMetadataRegistry.GetControl("VariableSizedWrapGrid");

        CollectionAssert.IsSubsetOf(
            new[] { "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            border.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "Background", "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            canvas.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "ColumnDefinitions", "RowDefinitions", "Margin", "Width" },
            grid.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "Background", "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            relativePanel.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[]
            {
                "Height",
                "HorizontalAlignment",
                "HorizontalScrollBarVisibility",
                "HorizontalScrollMode",
                "Margin",
                "VerticalAlignment",
                "VerticalScrollBarVisibility",
                "VerticalScrollMode",
                "Width"
            },
            scrollViewer.Properties.Select(property => property.Name).ToList());
        CollectionAssert.IsSubsetOf(
            new[] { "Background", "Height", "HorizontalAlignment", "Margin", "VerticalAlignment", "Width" },
            wrapGrid.Properties.Select(property => property.Name).ToList());
        Assert.AreEqual("Child", border.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Single, border.Content.Kind);
        Assert.AreEqual("Children", canvas.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Collection, canvas.Content.Kind);
        Assert.AreEqual("Children", grid.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Collection, grid.Content.Kind);
        Assert.AreEqual("Children", relativePanel.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Collection, relativePanel.Content.Kind);
        Assert.AreEqual("Content", scrollViewer.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Single, scrollViewer.Content.Kind);
        Assert.AreEqual("Children", wrapGrid.Content.DefaultPropertyName);
        Assert.AreEqual(ControlContentKind.Collection, wrapGrid.Content.Kind);
    }

    [TestMethod]
    public void AttachedPropertyRegistry_ContainsExpandedBuiltInSlice()
    {
        var names = AttachedPropertyMetadataRegistry.Properties
            .Select(property => property.QualifiedName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                "AutomationProperties.AutomationId",
                "AutomationProperties.HelpText",
                "AutomationProperties.ItemStatus",
                "AutomationProperties.ItemType",
                "AutomationProperties.LabeledBy",
                "AutomationProperties.Name",
                "Canvas.Left",
                "Canvas.Top",
                "Canvas.ZIndex",
                "Grid.Column",
                "Grid.ColumnSpan",
                "Grid.Row",
                "Grid.RowSpan",
                "RelativePanel.AlignLeftWithPanel",
                "RelativePanel.AlignTopWithPanel",
                "RelativePanel.Below",
                "RelativePanel.RightOf",
                "ScrollViewer.HorizontalScrollBarVisibility",
                "ScrollViewer.HorizontalScrollMode",
                "ScrollViewer.VerticalScrollBarVisibility",
                "ScrollViewer.VerticalScrollMode",
                "ToolTipService.ToolTip",
                "VariableSizedWrapGrid.ColumnSpan",
                "VariableSizedWrapGrid.RowSpan"
            },
            names);

        Assert.AreEqual(
            "Grid",
            AttachedPropertyMetadataRegistry.GetProperty("Grid", "Row").RequiredParentTagName);
        Assert.AreEqual(
            ValueKindHint.String,
            AttachedPropertyMetadataRegistry.GetProperty("AutomationProperties", "Name").ValueKindHint);
        Assert.AreEqual(
            "LeftProperty",
            AttachedPropertyMetadataRegistry.GetProperty("Canvas", "Left").DependencyPropertyFieldName);
        Assert.AreEqual(
            ValueKindHint.Bool,
            AttachedPropertyMetadataRegistry.GetProperty("RelativePanel", "AlignTopWithPanel").ValueKindHint);
        Assert.AreEqual(
            ValueKindHint.Enum,
            AttachedPropertyMetadataRegistry.GetProperty("ScrollViewer", "VerticalScrollMode").ValueKindHint);
        Assert.AreEqual(
            "VariableSizedWrapGrid",
            AttachedPropertyMetadataRegistry.GetProperty("VariableSizedWrapGrid", "RowSpan").RequiredParentTagName);
    }

    private static ValueKindHint ParseStyleHint()
    {
        return Enum.Parse<ValueKindHint>("Style");
    }

    private static void AssertCommonEventArgs(ControlMetadata metadata)
    {
        AssertEventArgs(
            metadata,
            "OnLoaded",
            "System.Action<Microsoft.UI.Xaml.RoutedEventArgs>");
        AssertEventArgs(
            metadata,
            "OnKeyDown",
            "System.Action<Microsoft.UI.Xaml.Input.KeyRoutedEventArgs>");
        AssertEventArgs(
            metadata,
            "OnPointerPressed",
            "System.Action<Microsoft.UI.Xaml.Input.PointerRoutedEventArgs>");
    }

    private static void AssertEventArgs(
        ControlMetadata metadata,
        string exposedName,
        string handlerTypeName)
    {
        var eventMetadata = metadata.Events.Single(@event => @event.ExposedName == exposedName);
        Assert.AreEqual(handlerTypeName, eventMetadata.HandlerTypeName);
        Assert.AreEqual(EventBindingKind.EventArgs, eventMetadata.BindingKind);
    }
}
