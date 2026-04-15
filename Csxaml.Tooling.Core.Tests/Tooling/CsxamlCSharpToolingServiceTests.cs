using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Diagnostics;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlCSharpToolingServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Completion_suggests_helper_symbols_inside_component_code()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                State<string> SelectedId = new State<string>("todo-1");

                var current = SelectedI;

                render <TextBlock Text={current} />;
            }
            """);
        var position = tempFile.Text.IndexOf("SelectedI", StringComparison.Ordinal) + "SelectedI".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "SelectedId"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_members_inside_expression_islands()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                State<string> SelectedId = new State<string>("todo-1");

                render <TextBlock Text={SelectedId.Val} />;
            }
            """);
        var position = tempFile.Text.IndexOf("SelectedId.Val", StringComparison.Ordinal) + "SelectedId.Val".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "Value"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Diagnostics_surface_csharp_errors_from_projected_regions()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                var current = MissingSymbol;

                render <TextBlock Text={current} />;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsTrue(
            diagnostics.Any(diagnostic => diagnostic.Message.Contains("MissingSymbol", StringComparison.Ordinal)),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }

    [TestMethod]
    public void Diagnostics_allow_valid_csxaml_brush_style_and_thickness_coercions()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe() {
                render <Border
                    Background={TodoColors.NotDoneBackground}
                    Padding={12}>
                    <TextBlock Text="Hello" Foreground={TodoColors.DoneForeground} />
                    <Button Style={TodoStyles.CardActionButton} />
                </Border>;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsFalse(
            diagnostics.Any(),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }
}
