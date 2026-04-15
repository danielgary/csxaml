namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class HelperCodeEmissionTests
{
    [TestMethod]
    public void Emit_ComponentLocalHelperCode_WritesHelpersBeforeRootDeclaration()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                string BuildTitle()
                {
                    return "Todo";
                }

                render <TextBlock Text={BuildTitle()} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);
        var helperIndex = emitted.IndexOf("string BuildTitle()", StringComparison.Ordinal);
        var rootIndex = emitted.IndexOf("var rootNode =", StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, helperIndex);
        if (helperIndex >= rootIndex)
        {
            Assert.Fail("Component-local helper code should appear before the root node declaration.");
        }
    }

    [TestMethod]
    public void Emit_FileScopedNamespaceAndHelpers_PreserveFileScopeOrdering()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            namespace Demo.Components;

            file sealed class TodoFormatter
            {
                public static string Format(string value) => value;
            }

            component Element TodoBoard(string Title) {
                render <TextBlock Text={TodoFormatter.Format(Title)} />;
            }

            file enum TodoTone
            {
                Normal
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "namespace Demo.Components;");
        StringAssert.Contains(emitted, "file sealed class TodoFormatter");
        StringAssert.Contains(emitted, "file enum TodoTone");
        Assert.IsLessThan(
            emitted.IndexOf("public sealed record TodoBoardProps", StringComparison.Ordinal),
            emitted.IndexOf("file sealed class TodoFormatter", StringComparison.Ordinal));
        Assert.IsGreaterThan(
            emitted.IndexOf("public sealed class TodoBoardComponent", StringComparison.Ordinal),
            emitted.IndexOf("file enum TodoTone", StringComparison.Ordinal));
    }
}
