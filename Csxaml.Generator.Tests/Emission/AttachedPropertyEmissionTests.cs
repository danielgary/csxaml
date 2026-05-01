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
            using Microsoft.UI.Xaml.Automation;

            component Element TodoEditor(string Title) {
                render <Border AutomationProperties.Name="Inner Editor">
                    <TextBlock Text={Title} />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Automation;
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid RowDefinitions="Auto,*">
                    <TextBlock Grid.Row={0} AutomationProperties.Name="Board Title" Text="Todo Board" />
                    <TodoEditor Grid.Row={1} Grid.Column={1} AutomationProperties.Name="Task Editor" Title="Draft plan" />
                </Grid>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(editor, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(emitted, "new NativeAttachedPropertyValue(");
        StringAssert.Contains(emitted, "\"Grid\"");
        StringAssert.Contains(emitted, "\"Row\"");
        StringAssert.Contains(emitted, "\"AutomationProperties\"");
        StringAssert.Contains(emitted, "\"Name\"");
        StringAssert.Contains(emitted, "\"Board Title\"");
        StringAssert.Contains(emitted, "new ComponentNode(");
        StringAssert.Contains(emitted, "typeof(global::TestProject.TodoEditorComponent)");
        StringAssert.Contains(emitted, "new global::TestProject.TodoEditorProps");
        StringAssert.Contains(emitted, "\"Draft plan\"");
        StringAssert.Contains(emitted, "new NativeAttachedPropertyValue[]");
    }

    [TestMethod]
    public void Emit_ExpandedAttachedProperties_PreserveOwnerPropertyAndHints()
    {
        var component = GeneratorTestHarness.Parse(
            "AttachedSurface.csxaml",
            """
            using Microsoft.UI.Xaml.Automation;
            using Microsoft.UI.Xaml.Controls;

            component Element AttachedSurface {
                render <Canvas>
                    <Button
                        Canvas.Left={20.5}
                        Canvas.Top={12}
                        ToolTipService.ToolTip="Move"
                        AutomationProperties.HelpText="Moves the item"
                        Content="Move" />
                </Canvas>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(component);
        var emitted = new CodeEmitter().Emit(component, compilation);

        StringAssert.Contains(emitted, "\"Canvas\"");
        StringAssert.Contains(emitted, "\"Left\"");
        StringAssert.Contains(emitted, "ValueKindHint.Double");
        StringAssert.Contains(emitted, "\"ToolTipService\"");
        StringAssert.Contains(emitted, "\"ToolTip\"");
        StringAssert.Contains(emitted, "\"AutomationProperties\"");
        StringAssert.Contains(emitted, "\"HelpText\"");
    }
}
