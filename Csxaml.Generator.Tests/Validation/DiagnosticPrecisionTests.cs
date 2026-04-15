namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class DiagnosticPrecisionTests
{
    [TestMethod]
    public void Validate_InvalidNativeProp_UsesAttributeSpanAndSuggestion()
    {
        var sourceText = GeneratorTestHarness.Normalize(
            """
            component Element TodoBoard {
                return <Button Contnet="Save" />;
            }
            """);
        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));
        var expectedSpan = FindSpan(sourceText, "Contnet=\"Save\"");

        StringAssert.Contains(exception.Diagnostic.Message, "did you mean 'Content'?");
        AssertSpan(exception.Diagnostic, 2, expectedSpan.Column, expectedSpan.EndColumn);
    }

    [TestMethod]
    public void Validate_InvalidComponentProp_UsesAttributeSpanAndSuggestion()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title) {
                return <TextBlock Text={Title} />;
            }
            """);
        var boardText = GeneratorTestHarness.Normalize(
            """
            component Element TodoBoard {
                return <TodoCard Ttile="One" />;
            }
            """);
        var board = GeneratorTestHarness.Parse("TodoBoard.csxaml", boardText);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));
        var expectedSpan = FindSpan(boardText, "Ttile=\"One\"");

        StringAssert.Contains(exception.Diagnostic.Message, "did you mean 'Title'?");
        AssertSpan(exception.Diagnostic, 2, expectedSpan.Column, expectedSpan.EndColumn);
    }

    [TestMethod]
    public void Validate_UnsupportedTag_SuggestsVisibleTagName()
    {
        var sourceText = GeneratorTestHarness.Normalize(
            """
            component Element TodoBoard {
                return <StakPanel />;
            }
            """);
        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));
        var expectedSpan = FindSpan(sourceText, "StakPanel");

        StringAssert.Contains(exception.Diagnostic.Message, "did you mean 'StackPanel'?");
        AssertSpan(exception.Diagnostic, 2, expectedSpan.Column, expectedSpan.EndColumn);
    }

    private static void AssertSpan(Diagnostic diagnostic, int expectedLine, int expectedColumn, int expectedEndColumn)
    {
        Assert.AreEqual("TodoBoard.csxaml", diagnostic.FilePath);
        Assert.AreEqual(expectedLine, diagnostic.Line);
        Assert.AreEqual(expectedColumn, diagnostic.Column);
        Assert.AreEqual(expectedLine, diagnostic.EndLine);
        Assert.AreEqual(expectedEndColumn, diagnostic.EndColumn);
    }

    private static (int Column, int EndColumn) FindSpan(string text, string snippet)
    {
        var lineText = text.Split('\n')[1];
        var index = lineText.IndexOf(snippet, StringComparison.Ordinal);
        if (index < 0)
        {
            Assert.Fail($"Could not find snippet '{snippet}' in '{lineText}'.");
        }

        return (index + 1, index + snippet.Length + 1);
    }
}
