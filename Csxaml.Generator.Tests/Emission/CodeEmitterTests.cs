namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class CodeEmitterTests
{
    [TestMethod]
    public void Emit_TodoCard_MatchesSnapshot()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                return <Border Background={IsDone ? TodoColors.DoneBackground : TodoColors.NotDoneBackground} Padding={TodoColors.CardPadding}>
                    <StackPanel Spacing={8}>
                    <TextBlock Text={Title} Foreground={TodoColors.CardForeground} />
                    if (IsDone) {
                        <TextBlock Text="Done" />
                    }
                    <Button Content="Toggle" OnClick={OnToggle} />
                    </StackPanel>
                </Border>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);
        const string expected = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Csxaml.Runtime;

            namespace TestProject;

            public sealed record TodoCardProps(string Title, bool IsDone, Action OnToggle);

            public sealed class TodoCardComponent : ComponentInstance<TodoCardProps>
            {
                private string Title => Props.Title;
                private bool IsDone => Props.IsDone;
                private Action OnToggle => Props.OnToggle;

                public override Node Render()
                {
                    var children0 = new List<Node>();
                    var children2 = new List<Node>();
                    var childNode3 = new NativeElementNode("TextBlock", null, new NativePropertyValue[] { new NativePropertyValue("Text", Title, global::Csxaml.ControlMetadata.ValueKindHint.String), new NativePropertyValue("Foreground", TodoColors.CardForeground, global::Csxaml.ControlMetadata.ValueKindHint.Brush) }, Array.Empty<NativeEventValue>(), Array.Empty<Node>());
                    children2.Add(childNode3);
                    if (IsDone)
                    {
                        var childNode4 = new NativeElementNode("TextBlock", null, new NativePropertyValue[] { new NativePropertyValue("Text", "Done", global::Csxaml.ControlMetadata.ValueKindHint.String) }, Array.Empty<NativeEventValue>(), Array.Empty<Node>());
                        children2.Add(childNode4);
                    }
                    var childNode5 = new NativeElementNode("Button", null, new NativePropertyValue[] { new NativePropertyValue("Content", "Toggle", global::Csxaml.ControlMetadata.ValueKindHint.Object) }, new NativeEventValue[] { new NativeEventValue("OnClick", (global::System.Action)(OnToggle), global::Csxaml.ControlMetadata.ValueKindHint.Unknown) }, Array.Empty<Node>());
                    children2.Add(childNode5);
                    var childNode1 = new NativeElementNode("StackPanel", null, new NativePropertyValue[] { new NativePropertyValue("Spacing", 8, global::Csxaml.ControlMetadata.ValueKindHint.Double) }, Array.Empty<NativeEventValue>(), children2);
                    children0.Add(childNode1);
                    var rootNode = new NativeElementNode("Border", null, new NativePropertyValue[] { new NativePropertyValue("Background", IsDone ? TodoColors.DoneBackground : TodoColors.NotDoneBackground, global::Csxaml.ControlMetadata.ValueKindHint.Brush), new NativePropertyValue("Padding", TodoColors.CardPadding, global::Csxaml.ControlMetadata.ValueKindHint.Thickness) }, Array.Empty<NativeEventValue>(), children0);
                    return rootNode;
                }
            }
            """;

        Assert.AreEqual(
            GeneratorTestHarness.Normalize(expected),
            GeneratorTestHarness.Normalize(emitted));
    }

    [TestMethod]
    public void Emit_TodoBoard_IncludesComponentNodeAndKeyedLoop()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                return <StackPanel>
                    <TextBlock Text={Title} />
                    <Button Content="Toggle" OnClick={OnToggle} />
                </StackPanel>;
            }
            """);

        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateItems());

                return <StackPanel>
                    foreach (var item in Items.Value) {
                        <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} OnToggle={OnToggle} />
                    }
                </StackPanel>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(card, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(emitted, "foreach (var item in Items.Value)");
        StringAssert.Contains(
            emitted,
            "new ComponentNode(typeof(global::TestProject.TodoCardComponent)");
        StringAssert.Contains(emitted, "\"position0\", item.Id");
    }

    [TestMethod]
    public void Emit_TodoEditor_IncludesProjectedInputEventHints()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoEditor.csxaml",
            """
            component Element TodoEditor(string Title, bool IsDone, Action<string> OnTitleChanged, Action<bool> OnDoneChanged) {
                return <StackPanel>
                    <TextBox Text={Title} OnTextChanged={OnTitleChanged} />
                    <CheckBox IsChecked={IsDone} OnCheckedChanged={OnDoneChanged} />
                </StackPanel>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(
            emitted,
            "new NativeEventValue(\"OnTextChanged\", (global::System.Action<string>)(OnTitleChanged), global::Csxaml.ControlMetadata.ValueKindHint.String)");
        StringAssert.Contains(
            emitted,
            "new NativeEventValue(\"OnCheckedChanged\", (global::System.Action<bool>)(OnDoneChanged), global::Csxaml.ControlMetadata.ValueKindHint.Bool)");
    }

    [TestMethod]
    public void Emit_BuiltInStyleExpression_UsesStyleValueKind()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <Button Content="Save" Style={TodoStyles.PrimaryButton} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(
            emitted,
            "new NativePropertyValue(\"Style\", TodoStyles.PrimaryButton, global::Csxaml.ControlMetadata.ValueKindHint.Style)");
    }
}
