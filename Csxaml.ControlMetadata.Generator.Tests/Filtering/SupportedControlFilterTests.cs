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
        Assert.AreEqual(ControlChildKind.None, metadata.ChildKind);
    }

    [TestMethod]
    public void GeneratedRegistry_ContainsExpectedCuratedControls()
    {
        var names = ControlMetadataRegistry.Controls
            .Select(control => control.TagName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { "Border", "Button", "StackPanel", "TextBlock" },
            names);
    }
}
