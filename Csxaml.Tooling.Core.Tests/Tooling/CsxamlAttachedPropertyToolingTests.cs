using Csxaml.Tooling.Core.CodeActions;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Hover;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlAttachedPropertyToolingTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Completion_suggests_expanded_attached_properties()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Button Canvas.L />;
            }
            """);
        var position = tempFile.Text.IndexOf("Canvas.L", StringComparison.Ordinal) + "Canvas.L".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "Canvas.Left"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Hover_returns_expanded_attached_property_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Canvas>
                    <Button Canvas.Left={20.5} />
                </Canvas>;
            }
            """);
        var position = tempFile.Text.IndexOf("Canvas.Left", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Attached property");
        StringAssert.Contains(hover.Markdown, "Canvas.Left");
        StringAssert.Contains(hover.Markdown, "Required parent: `<Canvas>`");
        StringAssert.Contains(hover.Markdown, "Dependency property: `LeftProperty`");
    }

    [TestMethod]
    public void Code_actions_suggest_expanded_attached_property_replacement()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using Microsoft.UI.Xaml.Automation;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock AutomationProperties.HelpTxt="Total help" />;
            }
            """);
        var range = FindRange(tempFile.Text, "AutomationProperties.HelpTxt");

        var actions = new CsxamlCodeActionService().GetCodeActions(
            tempFile.FilePath,
            tempFile.Text,
            range.Start.Line,
            range.Start.Character,
            range.End.Line,
            range.End.Character);

        Assert.HasCount(1, actions);
        Assert.AreEqual(
            "Replace 'AutomationProperties.HelpTxt' with 'AutomationProperties.HelpText'",
            actions[0].Title);
        Assert.AreEqual("AutomationProperties.HelpText", actions[0].Edits[0].NewText);
    }

    [TestMethod]
    public void Semantic_tokens_classify_expanded_attached_properties()
    {
        const string text =
            """
            using Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Canvas>
                    <Button Canvas.Left={20.5} />
                </Canvas>;
            }
            """;
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var attributeStart = text.IndexOf("Canvas.Left", StringComparison.Ordinal);

        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == attributeStart &&
                token.Length == "Canvas.Left".Length &&
                token.Type == CsxamlSemanticTokenType.Property),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }

    private static ((int Line, int Character) Start, (int Line, int Character) End) FindRange(
        string text,
        string value)
    {
        var start = text.IndexOf(value, StringComparison.Ordinal);
        return (GetLineAndCharacter(text, start), GetLineAndCharacter(text, start + value.Length));
    }

    private static (int Line, int Character) GetLineAndCharacter(string text, int offset)
    {
        var line = 0;
        var character = 0;
        for (var index = 0; index < offset && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                character = 0;
                continue;
            }

            if (text[index] != '\r')
            {
                character++;
            }
        }

        return (line, character);
    }
}
