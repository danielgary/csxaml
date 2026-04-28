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
            namespace Csxaml.Demo;

            component Element ToolingCacheCurrentOne() {
                render <TextBlock Text="One" />;
            }
            """;
        const string updatedText =
            """
            namespace Csxaml.Demo;

            component Element ToolingCacheCurrentTwo() {
                render <TextBlock Text="Two" />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            initialText);

        var loader = new CsxamlWorkspaceLoader();

        var updatedWorkspace = loader.Load(tempFile.FilePath, updatedText);

        Assert.IsTrue(updatedWorkspace.FindComponents("Csxaml.Demo", "ToolingCacheCurrentTwo").Any());
        Assert.IsFalse(updatedWorkspace.FindComponents("Csxaml.Demo", "ToolingCacheCurrentOne").Any());
    }

    [TestMethod]
    public void Workspace_loader_refreshes_cached_sibling_component_when_file_changes_on_disk()
    {
        const string currentText =
            """
            namespace Csxaml.Demo;

            component Element ToolingCacheHost() {
                render <TextBlock Text="Host" />;
            }
            """;
        const string siblingInitialText =
            """
            namespace Csxaml.Demo;

            component Element ToolingCacheSiblingOne() {
                render <TextBlock Text="Sibling One" />;
            }
            """;
        const string siblingUpdatedText =
            """
            namespace Csxaml.Demo;

            component Element ToolingCacheSiblingTwo() {
                render <TextBlock Text="Sibling Two" />;
            }
            """;

        using var currentFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            currentText);
        using var siblingFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            siblingInitialText);

        var loader = new CsxamlWorkspaceLoader();

        var initialWorkspace = loader.Load(currentFile.FilePath, currentText);
        Assert.IsTrue(initialWorkspace.FindComponents("Csxaml.Demo", "ToolingCacheSiblingOne").Any());

        File.WriteAllText(siblingFile.FilePath, siblingUpdatedText);

        var updatedWorkspace = loader.Load(currentFile.FilePath, currentText);

        Assert.IsTrue(updatedWorkspace.FindComponents("Csxaml.Demo", "ToolingCacheSiblingTwo").Any());
        Assert.IsFalse(updatedWorkspace.FindComponents("Csxaml.Demo", "ToolingCacheSiblingOne").Any());
    }
}
