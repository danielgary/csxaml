namespace Csxaml.Benchmarks;

internal static class SourceScenarioLibrary
{
    // Generator scenarios stay as real .csxaml source text so parser, validator,
    // component resolution, and emission all remain in the measured path.
    public static GeneratorScenario MediumComponent { get; } = new(
        Name: "medium-component",
        AssemblyName: "Csxaml.Benchmarks.Generated",
        DefaultComponentNamespace: "Benchmarks.Components",
        Files:
        [
            new GeneratorScenarioFile("TodoBoard.csxaml", MediumComponentSource),
        ]);

    public static GeneratorScenario MultiFileProject { get; } = new(
        Name: "multi-file-project",
        AssemblyName: "Csxaml.Benchmarks.Generated",
        DefaultComponentNamespace: "Benchmarks.Components",
        Files:
        [
            new GeneratorScenarioFile("TodoCard.csxaml", TodoCardSource),
            new GeneratorScenarioFile("TodoPanel.csxaml", TodoPanelSource),
            new GeneratorScenarioFile("TodoBoard.csxaml", TodoBoardSource),
        ]);

    private const string MediumComponentSource =
    """
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.UI.Xaml.Automation;

    namespace Benchmarks.Components;

    component Element TodoBoard(string Header) {
        inject ITodoService todoService;

        State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(todoService.LoadItems());
        State<string> SelectedId = new State<string>(Items.Value[0].Id);

        TodoItemModel Selected() => Items.Value.Single(item => item.Id == SelectedId.Value);
        string HeaderText() => $"{Header} ({Items.Value.Count})";

        render <StackPanel Spacing={12} AutomationProperties.Name={HeaderText()}>
            <TextBlock Text={HeaderText()} FontSize={24} />
            foreach (var item in Items.Value) {
                <Border Key={item.Id}>
                    <TextBlock Text={item.Title} />
                </Border>
            }
        </StackPanel>;
    }
    """;

    private const string TodoCardSource =
    """
    namespace Benchmarks.Components;

    component Element TodoCard(string Title, bool IsDone) {
        render <Border>
            <StackPanel>
                <TextBlock Text={Title} />
                if (IsDone) {
                    <TextBlock Text="Done" />
                }
            </StackPanel>
        </Border>;
    }
    """;

    private const string TodoPanelSource =
    """
    namespace Benchmarks.Components;

    component Element TodoPanel(string Heading) {
        render <Border>
            <StackPanel>
                <TextBlock Text={Heading} />
                <Slot />
            </StackPanel>
        </Border>;
    }
    """;

    private const string TodoBoardSource =
    """
    using System.Collections.Generic;

    namespace Benchmarks.Components;

    component Element TodoBoard {
        State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateItems());

        List<TodoItemModel> CreateItems() => new();

        render <TodoPanel Heading="Board">
            foreach (var item in Items.Value) {
                <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} />
            }
        </TodoPanel>;
    }
    """;
}
