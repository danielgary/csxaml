namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class CSharpIslandParserTests
{
    [TestMethod]
    public void Parse_PropertyExpression_WithInterpolatedRawString_KeepsIslandBoundaries()
    {
        const string sourceText = """"
            component Element TodoBoard {
                render <TextBlock Text={$$"""Count: {{{1 + 1}}}"""} />;
            }
            """";

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;
        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.AreEqual("TextBlock", root.TagName);
        Assert.AreEqual("$$\"\"\"Count: {{{1 + 1}}}\"\"\"", root.Properties[0].ValueText);
    }

    [TestMethod]
    public void Parse_HelperCode_IgnoresRenderTokensInsideRawStringLiterals()
    {
        const string sourceText = """"
            component Element TodoBoard {
                var raw = """render <Broken />""";
                var interpolated = $$"""render <StillBroken /> {{{1 + 1}}}""";

                render <TextBlock Text="Todo" />;
            }
            """";

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;
        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.IsNotNull(component.HelperCode);
        StringAssert.Contains(component.HelperCode!.CodeText, "\"\"\"render <Broken />\"\"\"");
        StringAssert.Contains(component.HelperCode.CodeText, "$$\"\"\"render <StillBroken /> {{{1 + 1}}}\"\"\"");
        Assert.AreEqual("TextBlock", root.TagName);
    }
}
