using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator.Tests.Discovery;

[TestClass]
public sealed class ControlMetadataDiscovererTests
{
    [TestMethod]
    public void Discover_Button_FindsCuratedPropertiesAndEvents()
    {
        var discovered = new ControlMetadataDiscoverer().Discover(typeof(Button));

        CollectionAssert.Contains(discovered.Properties.Keys.ToList(), "Content");
        CollectionAssert.Contains(discovered.Properties.Keys.ToList(), "Background");
        CollectionAssert.Contains(discovered.Events.Keys.ToList(), "Click");
    }
}
