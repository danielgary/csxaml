namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class AttachedPropertyEmissionTests
{
    [TestMethod]
    public void Emit_AttachedProperties_UseDedicatedRuntimeValues()
    {
        var editor = GeneratorTestHarness.Parse(
            "TodoEditor.csxaml",
            """
            component Element TodoEditor(string Title) {
                return <Border AutomationProperties.Name="Inner Editor">
                    <TextBlock Text={Title} />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <Grid RowDefinitions="Auto,*">
                    <TextBlock Grid.Row={0} AutomationProperties.Name="Board Title" Text="Todo Board" />
                    <TodoEditor Grid.Row={1} Grid.Column={1} AutomationProperties.Name="Task Editor" Title="Draft plan" />
                </Grid>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(editor, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(
            emitted,
            "new NativeAttachedPropertyValue(\"Grid\", \"Row\", 0, global::Csxaml.ControlMetadata.ValueKindHint.Int)");
        StringAssert.Contains(
            emitted,
            "new NativeAttachedPropertyValue(\"AutomationProperties\", \"Name\", \"Board Title\", global::Csxaml.ControlMetadata.ValueKindHint.String)");
        StringAssert.Contains(
            emitted,
            "new ComponentNode(typeof(global::TestProject.TodoEditorComponent), new global::TestProject.TodoEditorProps(\"Draft plan\"), Array.Empty<Node>(), new NativeAttachedPropertyValue[]");
    }
}
