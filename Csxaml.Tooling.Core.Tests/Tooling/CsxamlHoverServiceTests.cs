using Csxaml.Tooling.Core.Hover;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlHoverServiceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Hover_returns_component_tag_details()
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

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Component tag");
        StringAssert.Contains(hover.Markdown, "Csxaml.Samples.TodoApp.TodoCard");
        StringAssert.Contains(hover.Markdown, "Assembly:");
    }

    [TestMethod]
    public void Hover_returns_native_control_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Button />;
            }
            """);
        var position = tempFile.Text.IndexOf("Button", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "WinUI control tag");
        StringAssert.Contains(hover.Markdown, "Microsoft.UI.Xaml.Controls.Button");
    }

    [TestMethod]
    public void Hover_returns_native_property_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock Text="Hello" />;
            }
            """);
        var position = tempFile.Text.IndexOf("Text=", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Native property");
        StringAssert.Contains(hover.Markdown, "TextBlock.Text");
    }

    [TestMethod]
    public void Hover_returns_native_event_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Button OnClick={() => SaveChanges()} />;
            }
            """);
        var position = tempFile.Text.IndexOf("OnClick", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Native event");
        StringAssert.Contains(hover.Markdown, "Button.OnClick");
    }

    [TestMethod]
    public void Hover_returns_typed_event_args_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Slider OnValueChanged={args => _ = args.NewValue} />;
            }
            """);
        var position = tempFile.Text.IndexOf("OnValueChanged", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Native event");
        StringAssert.Contains(hover.Markdown, "Slider.OnValueChanged");
        StringAssert.Contains(hover.Markdown, "EventArgs");
        StringAssert.Contains(hover.Markdown, "RangeBaseValueChangedEventArgs");
    }

    [TestMethod]
    public void Hover_returns_element_ref_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                ElementRef<object> SearchBox = new ElementRef<object>();

                render <TextBox Ref={SearchBox} />;
            }
            """);
        var position = tempFile.Text.IndexOf("Ref={", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Element reference");
        StringAssert.Contains(hover.Markdown, "TextBox.Ref");
        StringAssert.Contains(hover.Markdown, "ElementRef<Microsoft.UI.Xaml.Controls.TextBox>");
    }

    [TestMethod]
    public void Hover_returns_attached_property_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TextBlock Grid.Row={1} />;
            }
            """);
        var position = tempFile.Text.IndexOf("Grid.Row", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Attached property");
        StringAssert.Contains(hover.Markdown, "Grid.Row");
    }

    [TestMethod]
    public void Hover_returns_helper_method_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                int NextCount(int value)
                {
                    return value + 1;
                }

                var next = NextCount(3);
                render <TextBlock Text={next.ToString()} />;
            }
            """);
        var position = tempFile.Text.LastIndexOf("NextCount", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "C# method");
        StringAssert.Contains(hover.Markdown, "int NextCount(int value)");
    }

    [TestMethod]
    public void Hover_returns_injected_service_details_inside_state_initializer()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                inject ITodoService todoService;
                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(todoService.LoadItems());

                render <TextBlock Text={Items.Value[0].Title} />;
            }
            """);
        var position = tempFile.Text.IndexOf("todoService.LoadItems", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "C# field");
        StringAssert.Contains(hover.Markdown, "ITodoService todoService");
    }
}
