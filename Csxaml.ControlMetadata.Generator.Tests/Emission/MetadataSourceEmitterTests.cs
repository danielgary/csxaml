using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator.Tests.Emission;

[TestClass]
public sealed class MetadataSourceEmitterTests
{
    [TestMethod]
    public void Emit_CuratedControls_IsDeterministicAndIncludesButtonClick()
    {
        var definition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(Button));
        var discovered = new ControlMetadataDiscoverer().Discover(typeof(Button));
        var metadata = new SupportedControlFilter().BuildMetadata(definition, discovered);
        var emitter = new MetadataSourceEmitter();

        var first = emitter.Emit([metadata]);
        var second = emitter.Emit([metadata]);

        Assert.AreEqual(first, second);
        StringAssert.Contains(first, "new ControlMetadata(");
        StringAssert.Contains(first, "\"Button\"");
        StringAssert.Contains(first, "new EventMetadata(\"Click\", \"OnClick\"");
    }
}
