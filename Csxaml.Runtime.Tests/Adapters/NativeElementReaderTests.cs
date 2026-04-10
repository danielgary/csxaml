namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class NativeElementReaderTests
{
    [TestMethod]
    public void TryGetPropertyValue_DoubleHint_CoercesIntegralLiteral()
    {
        var node = new NativeElementNode(
            "StackPanel",
            null,
            [new NativePropertyValue("Spacing", 12, global::Csxaml.ControlMetadata.ValueKindHint.Double)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());

        var found = NativeElementReader.TryGetPropertyValue<double>(node, "Spacing", out var spacing);

        Assert.IsTrue(found);
        Assert.AreEqual(12d, spacing);
    }

    [TestMethod]
    public void TryGetPropertyValue_InvalidDoubleValue_Throws()
    {
        var node = new NativeElementNode(
            "StackPanel",
            null,
            [new NativePropertyValue("Spacing", "wide", global::Csxaml.ControlMetadata.ValueKindHint.Double)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());

        var error = Assert.ThrowsExactly<InvalidOperationException>(
            () => NativeElementReader.TryGetPropertyValue<double>(node, "Spacing", out _));

        StringAssert.Contains(error.Message, "Native property 'Spacing'");
    }
}
