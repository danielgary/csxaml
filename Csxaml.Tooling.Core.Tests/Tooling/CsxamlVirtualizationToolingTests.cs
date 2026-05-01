using Csxaml.Tooling.Core.Hover;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlVirtualizationToolingTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Hover_foreach_mentions_repeated_rendering_not_virtualization()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <StackPanel>
                    foreach (var item in Items.Value) {
                        <TextBlock Key={item.Id} Text={item.Title} />
                    }
                </StackPanel>;
            }
            """);
        var position = tempFile.Text.IndexOf("foreach", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Repeated child rendering");
        StringAssert.Contains(hover.Markdown, "not virtualization");
        StringAssert.Contains(hover.Markdown, "native virtualized control");
    }

    [TestMethod]
    public void Hover_foreach_in_helper_code_does_not_show_markup_guidance()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                void VisitItems()
                {
                    foreach (var item in Items.Value) {
                    }
                }

                render <StackPanel />;
            }
            """);
        var position = tempFile.Text.IndexOf("foreach", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        if (hover is not null)
        {
            Assert.IsFalse(
                hover.Markdown.Contains("Repeated child rendering", StringComparison.Ordinal),
                hover.Markdown);
        }
    }
}
