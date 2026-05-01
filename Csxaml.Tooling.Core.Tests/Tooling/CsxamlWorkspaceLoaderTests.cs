using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlWorkspaceLoaderTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Workspace_loader_uses_updated_current_text_for_open_document()
    {
        const string initialText =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingCacheCurrentOne() {
                render <TextBlock Text="One" />;
            }
            """;
        const string updatedText =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingCacheCurrentTwo() {
                render <TextBlock Text="Two" />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            initialText);

        var loader = new CsxamlWorkspaceLoader();

        var updatedWorkspace = loader.Load(tempFile.FilePath, updatedText);

        Assert.IsTrue(updatedWorkspace.FindComponents("Csxaml.Samples.TodoApp", "ToolingCacheCurrentTwo").Any());
        Assert.IsFalse(updatedWorkspace.FindComponents("Csxaml.Samples.TodoApp", "ToolingCacheCurrentOne").Any());
    }

    [TestMethod]
    public void Workspace_loader_refreshes_cached_sibling_component_when_file_changes_on_disk()
    {
        const string currentText =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingCacheHost() {
                render <TextBlock Text="Host" />;
            }
            """;
        const string siblingInitialText =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingCacheSiblingOne() {
                render <TextBlock Text="Sibling One" />;
            }
            """;
        const string siblingUpdatedText =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingCacheSiblingTwo() {
                render <TextBlock Text="Sibling Two" />;
            }
            """;

        using var currentFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            currentText);
        using var siblingFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            siblingInitialText);

        var loader = new CsxamlWorkspaceLoader();

        var initialWorkspace = loader.Load(currentFile.FilePath, currentText);
        Assert.IsTrue(initialWorkspace.FindComponents("Csxaml.Samples.TodoApp", "ToolingCacheSiblingOne").Any());

        File.WriteAllText(siblingFile.FilePath, siblingUpdatedText);

        var updatedWorkspace = loader.Load(currentFile.FilePath, currentText);

        Assert.IsTrue(updatedWorkspace.FindComponents("Csxaml.Samples.TodoApp", "ToolingCacheSiblingTwo").Any());
        Assert.IsFalse(updatedWorkspace.FindComponents("Csxaml.Samples.TodoApp", "ToolingCacheSiblingOne").Any());
    }
}
