namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ReferenceEqualityListTests
{
    [TestMethod]
    public void Matches_ReturnsTrue_WhenReferencesMatchInOrder()
    {
        var first = new object();
        var second = new object();

        var matches = ReferenceEqualityList.Matches(
            new[] { first, second },
            new[] { first, second });

        Assert.IsTrue(matches);
    }

    [TestMethod]
    public void Matches_ReturnsFalse_WhenOrderDiffers()
    {
        var first = new object();
        var second = new object();

        var matches = ReferenceEqualityList.Matches(
            new[] { first, second },
            new[] { second, first });

        Assert.IsFalse(matches);
    }

    [TestMethod]
    public void Matches_ReturnsFalse_WhenCountDiffers()
    {
        var first = new object();
        var second = new object();

        var matches = ReferenceEqualityList.Matches(
            new[] { first },
            new[] { first, second });

        Assert.IsFalse(matches);
    }
}
