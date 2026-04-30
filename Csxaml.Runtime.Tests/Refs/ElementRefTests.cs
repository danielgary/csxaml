namespace Csxaml.Runtime.Tests.Refs;

[TestClass]
public sealed class ElementRefTests
{
    [TestMethod]
    public void TryGet_UnmountedReference_ReturnsFalse()
    {
        var reference = new ElementRef<object>();

        var found = reference.TryGet(out var element);

        Assert.IsFalse(found);
        Assert.IsNull(element);
        Assert.IsNull(reference.Current);
    }
}
