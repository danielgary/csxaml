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
                    <StackPanel>
                        <TextBlock Text="Hello" Foreground={TodoColors.DoneForeground} />
                        <Button Style={TodoStyles.CardActionButton} />
                    </StackPanel>
                </Border>;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsFalse(
            diagnostics.Any(),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }

    [TestMethod]
    public void Diagnostics_allow_alias_imported_external_controls_in_demo_workspace()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            using Microsoft.UI.Xaml;
            using DemoControls = Csxaml.ExternalControls;
            using WinUi = Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Demo;

            component Element ToolingProbe {
                render <Grid>
                    <DemoControls:StatusButton BadgeText="ready">
                        <TextBlock Text="Ready" />
                    </DemoControls:StatusButton>
                    <WinUi:InfoBar
                        IsOpen={true}
                        Severity={WinUi.InfoBarSeverity.Informational}
                        Title="Interop"
                        Message="Works" />
                </Grid>;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsFalse(
            diagnostics.Any(diagnostic => diagnostic.Message.Contains("unsupported tag name", StringComparison.OrdinalIgnoreCase)),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }

    [TestMethod]
    public void Diagnostics_allow_csxaml_state_constructor_shape_in_helper_code()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe {
                State<List<string>> Items = new State<List<string>>(new List<string> { "A", "B" });
                State<string> SelectedItemId = new State<string>(Items.Value[0]);

                render <TextBlock Text={SelectedItemId.Value} />;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsFalse(
            diagnostics.Any(diagnostic =>
                diagnostic.Message.Contains("invalidate", StringComparison.OrdinalIgnoreCase) ||
                diagnostic.Message.Contains("State<List<string>>.State", StringComparison.OrdinalIgnoreCase) ||
                diagnostic.Message.Contains("State<string>.State", StringComparison.OrdinalIgnoreCase)),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }

    [TestMethod]
    public void Diagnostics_allow_state_initializers_to_reference_injected_services_and_prior_state_fields()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "Csxaml.Demo", "Components"),
            """
            namespace Csxaml.Demo;

            component Element ToolingProbe {
                inject ITodoService todoService;

                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(todoService.LoadItems());
                State<string> SelectedItemId = new State<string>(Items.Value[0].Id);

                render <TextBlock Text={SelectedItemId.Value} />;
            }
            """);

        var diagnostics = new CsxamlDiagnosticService().GetDiagnostics(tempFile.FilePath, tempFile.Text);

        Assert.IsFalse(
            diagnostics.Any(),
            $"Diagnostics: {string.Join(" | ", diagnostics.Select(diagnostic => diagnostic.Message))}");
    }
}
