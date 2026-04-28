using Csxaml.Tooling.Core.CodeActions;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlCodeActionServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Code_actions_offer_tag_replacement_for_unambiguous_suggestion()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                render <StakPanel />;
            }
            """);
        var expectedStart = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("StakPanel", StringComparison.Ordinal));
        var expectedEnd = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("StakPanel", StringComparison.Ordinal) + "StakPanel".Length);

        var actions = new CsxamlCodeActionService().GetCodeActions(tempFile.FilePath, tempFile.Text, 3, 12, 3, 21);

        Assert.HasCount(1, actions);
        Assert.AreEqual("Replace 'StakPanel' with 'StackPanel'", actions[0].Title);
        Assert.AreEqual("StackPanel", actions[0].Edits[0].NewText);
        Assert.AreEqual(expectedStart.Line, actions[0].Edits[0].StartLine);
        Assert.AreEqual(expectedStart.Character, actions[0].Edits[0].StartCharacter);
        Assert.AreEqual(expectedEnd.Character, actions[0].Edits[0].EndCharacter);
    }

    [TestMethod]
    public void Code_actions_offer_native_attribute_replacement_for_unambiguous_suggestion()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                render <Button Contnet="Save" />;
            }
            """);
        var expectedStart = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("Contnet", StringComparison.Ordinal));
        var expectedEnd = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("Contnet", StringComparison.Ordinal) + "Contnet".Length);

        var actions = new CsxamlCodeActionService().GetCodeActions(tempFile.FilePath, tempFile.Text, 3, 20, 3, 27);

        Assert.HasCount(1, actions);
        Assert.AreEqual("Replace 'Contnet' with 'Content'", actions[0].Title);
        Assert.AreEqual("Content", actions[0].Edits[0].NewText);
        Assert.AreEqual(expectedStart.Line, actions[0].Edits[0].StartLine);
        Assert.AreEqual(expectedStart.Character, actions[0].Edits[0].StartCharacter);
        Assert.AreEqual(expectedEnd.Character, actions[0].Edits[0].EndCharacter);
    }

    [TestMethod]
    public void Code_actions_offer_component_parameter_replacement_for_unambiguous_suggestion()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                render <TodoCard Ttile="One" />;
            }
            """);
        var expectedStart = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("Ttile", StringComparison.Ordinal));
        var expectedEnd = GetLineAndCharacter(tempFile.Text, tempFile.Text.IndexOf("Ttile", StringComparison.Ordinal) + "Ttile".Length);

        var actions = new CsxamlCodeActionService().GetCodeActions(tempFile.FilePath, tempFile.Text, 3, 22, 3, 27);

        Assert.HasCount(1, actions);
        Assert.AreEqual("Replace 'Ttile' with 'Title'", actions[0].Title);
        Assert.AreEqual("Title", actions[0].Edits[0].NewText);
        Assert.AreEqual(expectedStart.Line, actions[0].Edits[0].StartLine);
        Assert.AreEqual(expectedStart.Character, actions[0].Edits[0].StartCharacter);
        Assert.AreEqual(expectedEnd.Character, actions[0].Edits[0].EndCharacter);
    }

    [TestMethod]
    public void Code_actions_do_not_offer_replacements_without_a_high_confidence_suggestion()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                render <Zzzzz />;
            }
            """);

        var actions = new CsxamlCodeActionService().GetCodeActions(tempFile.FilePath, tempFile.Text, 3, 12, 3, 17);

        Assert.IsEmpty(actions);
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
