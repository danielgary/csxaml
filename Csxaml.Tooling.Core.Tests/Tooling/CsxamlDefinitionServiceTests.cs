using Csxaml.Tooling.Core.Definitions;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlDefinitionServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Definition_resolves_helper_method_usages_to_same_file()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(new());

                TodoItemModel SelectedItem()
                {
                    return Items.Value.Single(item => item.Id == "todo-1");
                }

                var selected = SelectedItem();
                render <TextBlock Text={selected.Title} />;
            }
            """);
        var usagePosition = tempFile.Text.LastIndexOf("SelectedItem()", StringComparison.Ordinal) + 1;
        var declarationStart = tempFile.Text.IndexOf("SelectedItem()", StringComparison.Ordinal);

        var definition = new CsxamlDefinitionService().GetDefinition(tempFile.FilePath, tempFile.Text, usagePosition);

        Assert.IsNotNull(definition);
        Assert.AreEqual(tempFile.FilePath, definition.FilePath);
        Assert.AreEqual(declarationStart, definition.Start);
    }

    [TestMethod]
    public void Definition_resolves_project_types_in_helper_code_to_source_files()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                TodoItemModel SelectedItem()
                {
                    return new TodoItemModel("todo-1", "Draft plan", "Notes", false);
                }

                render <TextBlock Text="Hello" />;
            }
            """);
        var position = tempFile.Text.IndexOf("TodoItemModel SelectedItem", StringComparison.Ordinal) + 1;

        var definition = new CsxamlDefinitionService().GetDefinition(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(definition);
        StringAssert.EndsWith(definition.FilePath, "Csxaml.Demo\\Models\\TodoItemModel.cs");
    }
}
