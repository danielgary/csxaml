using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Definitions;
using Csxaml.Tooling.Core.Formatting;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlToolingServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Completion_suggests_workspace_component_tags()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TodoC />;
            }
            """);
        var position = tempFile.Text.IndexOf("TodoC", StringComparison.Ordinal) + "TodoC".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "TodoCard"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_component_parameters_as_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TodoCard Tit />;
            }
            """);
        var position = tempFile.Text.IndexOf("Tit", StringComparison.Ordinal) + "Tit".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "Title"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_native_events_as_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Button OnC />;
            }
            """);
        var position = tempFile.Text.IndexOf("OnC", StringComparison.Ordinal) + "OnC".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "OnClick"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_typed_event_args_as_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Slider OnV />;
            }
            """);
        var position = tempFile.Text.IndexOf("OnV", StringComparison.Ordinal) + "OnV".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);
        var valueChanged = items.SingleOrDefault(item => item.Label == "OnValueChanged");

        Assert.IsNotNull(valueChanged, $"Items: {string.Join(", ", items.Select(item => item.Label))}");
        StringAssert.Contains(
            valueChanged.Detail,
            "System.Action<Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs>");
    }

    [TestMethod]
    public void Completion_suggests_imported_external_control_tags_through_aliases()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using DemoControls = Csxaml.ExternalControls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <DemoControls:Stat />;
            }
            """);
        var position = tempFile.Text.IndexOf("DemoControls:Stat", StringComparison.Ordinal) + "DemoControls:Stat".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "DemoControls:StatusButton"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_package_controls_through_aliases()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using WinUi = Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <WinUi:Info />;
            }
            """);
        var position = tempFile.Text.IndexOf("WinUi:Info", StringComparison.Ordinal) + "WinUi:Info".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "WinUi:InfoBar"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_attached_properties_as_attributes()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock Grid.R />;
            }
            """);
        var position = tempFile.Text.IndexOf("Grid.R", StringComparison.Ordinal) + "Grid.R".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "Grid.Row"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Completion_suggests_injected_services_in_helper_code()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                inject ITodoService todoService;
                var items = todoS;
                render <TextBlock Text="Hello" />;
            }
            """);
        var position = tempFile.Text.IndexOf("var items = todoS", StringComparison.Ordinal) + "var items = todoS".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(
            items.Any(item => item.Label == "todoService"),
            $"Items: {string.Join(", ", items.Select(item => item.Label))}");
    }

    [TestMethod]
    public void Definition_resolves_component_usages_to_source_files()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TodoCard />;
            }
            """);
        var position = tempFile.Text.IndexOf("TodoCard", StringComparison.Ordinal) + 1;

        var definition = new CsxamlDefinitionService().GetDefinition(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(definition);
        StringAssert.EndsWith(definition.FilePath, "samples\\Csxaml.TodoApp\\Components\\TodoCard.csxaml");
    }

    [TestMethod]
    public void Semantic_tokens_classify_attached_properties_as_properties()
    {
        const string text =
            """
            using Microsoft.UI.Xaml.Controls;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock Grid.Row={1} />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var attributeStart = text.IndexOf("Grid.Row", StringComparison.Ordinal);

        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == attributeStart &&
                token.Length == "Grid.Row".Length &&
                token.Type == CsxamlSemanticTokenType.Property),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }

    [TestMethod]
    public void Semantic_tokens_do_not_classify_invisible_attached_properties()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock Grid.Row={1} />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var attributeStart = text.IndexOf("Grid.Row", StringComparison.Ordinal);

        Assert.IsFalse(
            tokens.Any(token =>
                token.Start == attributeStart &&
                token.Length == "Grid.Row".Length &&
                token.Type == CsxamlSemanticTokenType.Property),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }

    [TestMethod]
    public void Semantic_tokens_classify_render_and_inject_keywords_and_helper_code_symbols()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                inject ITodoService todoService;

                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(new());

                TodoItemModel SelectedItem()
                {
                    return Items.Value.Single(item => item.Id == "todo-1");
                }

                render <TextBlock Text="Hello" />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var injectStart = text.IndexOf("inject", StringComparison.Ordinal);
        var renderStart = text.IndexOf("render", StringComparison.Ordinal);
        var listStart = text.IndexOf("List<TodoItemModel>", StringComparison.Ordinal);
        var returnTypeStart = text.IndexOf("TodoItemModel SelectedItem", StringComparison.Ordinal);
        var methodStart = text.IndexOf("SelectedItem", StringComparison.Ordinal);

        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == injectStart &&
                token.Length == "inject".Length &&
                token.Type == CsxamlSemanticTokenType.Keyword),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == renderStart &&
                token.Length == "render".Length &&
                token.Type == CsxamlSemanticTokenType.Keyword),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == listStart &&
                token.Length == "List".Length &&
                token.Type == CsxamlSemanticTokenType.Class),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == returnTypeStart &&
                token.Length == "TodoItemModel".Length &&
                token.Type == CsxamlSemanticTokenType.Class),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == methodStart &&
                token.Length == "SelectedItem".Length &&
                token.Type == CsxamlSemanticTokenType.Method),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }

    [TestMethod]
    public void Formatting_normalizes_indentation_for_mixed_markup_and_control_flow()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
            render <Border
            Background={TodoColors.NotDoneBackground}>
            if (true) {
            <TextBlock Text="Hello" />
            }
            </Border>;
            }
            """;

        var formatted = new CsxamlFormattingService().FormatDocument(text);

        StringAssert.Contains(
            formatted,
            """
                render <Border
                    Background={TodoColors.NotDoneBackground}>
                    if (true) {
                        <TextBlock Text="Hello" />
                    }
                </Border>;
            """);
    }
}
