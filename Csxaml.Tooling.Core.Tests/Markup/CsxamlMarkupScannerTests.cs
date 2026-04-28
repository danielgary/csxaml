using Csxaml.Tooling.Core.Markup;

namespace Csxaml.Tooling.Core.Tests.Markup;

[TestClass]
public sealed class CsxamlMarkupScannerTests
{
    [TestMethod]
    public void Scan_DottedTag_PreservesFullTagName()
    {
        var scan = CsxamlMarkupScanner.Scan(
            """
            component Element TodoBoard {
                render <Foo.Bar></Foo.Bar>;
            }
            """);

        Assert.AreEqual("Foo.Bar", scan.Elements[0].TagName);
        Assert.AreEqual("Foo.Bar", scan.Elements[0].LocalName);
        Assert.IsNull(scan.Elements[0].Prefix);
    }

    [TestMethod]
    public void Scan_RawStringExpression_DoesNotTerminateMarkupEarly()
    {
        var scan = CsxamlMarkupScanner.Scan(
            """"
            component Element TodoBoard {
                render <TextBlock Text={$$"""Count > {{{1 + 1}}}"""} />;
            }
            """");

        Assert.AreEqual("TextBlock", scan.Elements[0].TagName);
        CollectionAssert.AreEqual(
            new[] { "Text" },
            scan.Elements[0].Attributes.Select(attribute => attribute.Name).ToArray());
    }
}
