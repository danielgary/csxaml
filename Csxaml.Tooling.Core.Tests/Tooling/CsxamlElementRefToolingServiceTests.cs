using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlElementRefToolingServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Completion_suggests_ref_for_native_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBox Re />;
            }
            """);
        var position = tempFile.Text.IndexOf("Re", StringComparison.Ordinal) + "Re".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);
        var item = items.SingleOrDefault(item => item.Label == "Ref");

        Assert.IsNotNull(item, $"Items: {string.Join(", ", items.Select(item => item.Label))}");
        StringAssert.Contains(item.Detail, "Csxaml.Runtime.ElementRef<Microsoft.UI.Xaml.Controls.TextBox>");
    }

    [TestMethod]
    public void Completion_does_not_suggest_ref_for_component_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TodoCard Re />;
            }
            """);
        var position = tempFile.Text.IndexOf("Re", StringComparison.Ordinal) + "Re".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsFalse(
            items.Any(item => item.Label == "Ref"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Semantic_tokens_classify_ref_as_readonly_property()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                ElementRef<object> SearchBox = new ElementRef<object>();

                render <TextBox Ref={SearchBox} />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var attributeStart = text.IndexOf("Ref={", StringComparison.Ordinal);

        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == attributeStart &&
                token.Length == "Ref".Length &&
                token.Type == CsxamlSemanticTokenType.Property &&
                token.IsReadOnly),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }
}
