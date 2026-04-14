using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator.Tests.Emission;

[TestClass]
public sealed class MetadataSourceEmitterTests
{
    [TestMethod]
    public void Emit_CuratedControls_IsDeterministicAndIncludesProjectedEventMetadata()
    {
        var buttonDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(Button));
        var buttonDiscovered = new ControlMetadataDiscoverer().Discover(typeof(Button));
        var buttonMetadata = new SupportedControlFilter().BuildMetadata(buttonDefinition, buttonDiscovered);
        var textBoxDefinition = CuratedControlSet.Definitions.Single(
            control => control.ControlType == typeof(TextBox));
        var textBoxDiscovered = new ControlMetadataDiscoverer().Discover(typeof(TextBox));
        var textBoxMetadata = new SupportedControlFilter().BuildMetadata(textBoxDefinition, textBoxDiscovered);
        var emitter = new MetadataSourceEmitter();

        var first = emitter.Emit([buttonMetadata, textBoxMetadata]);
        var second = emitter.Emit([buttonMetadata, textBoxMetadata]);

        Assert.AreEqual(first, second);
        StringAssert.Contains(first, "new ControlMetadata(");
        StringAssert.Contains(first, "\"Button\"");
        StringAssert.Contains(first, "new PropertyMetadata(\"Style\", \"Microsoft.UI.Xaml.Style\", true, true, false, true, ValueKindHint.Style)");
        StringAssert.Contains(first, "new EventMetadata(\"Click\", \"OnClick\", \"System.Action\", true, ValueKindHint.Unknown, EventBindingKind.Direct)");
        StringAssert.Contains(first, "new EventMetadata(\"TextChanged\", \"OnTextChanged\", \"System.Action<string>\", true, ValueKindHint.String, EventBindingKind.TextValueChanged)");
    }
}
