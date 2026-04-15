namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class HelperCodeParserTests
{
    [TestMethod]
    public void Parse_ComponentLocalHelperCode_CapturesRawCodeBeforeRenderStatement()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                State<int> Count = new State<int>(1);

                string BuildTitle()
                {
                    return $"Count:{Count.Value}";
                }

                render <TextBlock Text={BuildTitle()} />;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.IsNotNull(component.HelperCode);
        StringAssert.Contains(component.HelperCode!.CodeText, "string BuildTitle()");
        Assert.AreEqual("TextBlock", root.TagName);
    }

    [TestMethod]
    public void Parse_ComponentLocalHelperCode_IgnoresNestedRenderTokens()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                string BuildTitle()
                {
                    // render <Broken />;
                    return "Todo";
                }

                var text = "render <StillBroken />";

                render <TextBlock Text={BuildTitle()} />;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.IsNotNull(component.HelperCode);
        StringAssert.Contains(component.HelperCode!.CodeText, "return \"Todo\";");
        StringAssert.Contains(component.HelperCode.CodeText, "\"render <StillBroken />\"");
        Assert.AreEqual("TextBlock", root.TagName);
    }

    [TestMethod]
    public void Parse_FileScopedNamespaceAndHelpers_CapturesTopLevelBlocks()
    {
        var file = new Parser().Parse(
            new SourceDocument(
                "TodoCard.csxaml",
                GeneratorTestHarness.Normalize(
                    """
                    using System;
                    namespace Demo.Components;

                    file sealed class TodoFormatter
                    {
                        public static string Format(string value) => value;
                    }

                    component Element TodoCard(string Title) {
                        render <TextBlock Text={TodoFormatter.Format(Title)} />;
                    }

                    file enum TodoTone
                    {
                        Normal
                    }
                    """)));

        Assert.IsNotNull(file.Namespace);
        Assert.AreEqual("Demo.Components", file.Namespace!.NamespaceName);
        Assert.HasCount(2, file.HelperCodeBlocks);
        StringAssert.Contains(file.HelperCodeBlocks[0].CodeText, "file sealed class TodoFormatter");
        StringAssert.Contains(file.HelperCodeBlocks[1].CodeText, "file enum TodoTone");
    }
}
