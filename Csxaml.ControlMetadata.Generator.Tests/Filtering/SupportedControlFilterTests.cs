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
            new[] { "Background", "Content", "FontSize", "Foreground" },
            metadata.Properties.Select(property => property.Name).ToArray());
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
            new[] { "Border", "Button", "CheckBox", "StackPanel", "TextBlock", "TextBox" },
            names);
    }
}
