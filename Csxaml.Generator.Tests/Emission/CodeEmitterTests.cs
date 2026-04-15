namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class CodeEmitterTests
{
    [TestMethod]
    public void Emit_TodoCard_IncludesSourceContextAndLineDirectives()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                render <Border Background={IsDone ? TodoColors.DoneBackground : TodoColors.NotDoneBackground} Padding={TodoColors.CardPadding}>
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
        var document = new CodeEmitter().EmitDocument(component, GeneratorTestHarness.Validate(component));
        var emitted = document.Text;

        StringAssert.Contains(emitted, "#nullable enable");
        StringAssert.Contains(emitted, "public override CsxamlSourceInfo? CsxamlSourceInfo =>");
        StringAssert.Contains(emitted, "new global::Csxaml.Runtime.CsxamlSourceInfo(");
        StringAssert.Contains(emitted, "#line ");
        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "native-tag" && entry.Label == "Border"));
        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "if-block" && entry.Label == "if"));
    }

    [TestMethod]
    public void Emit_TodoBoard_IncludesComponentNodeAndKeyedLoop()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                render <StackPanel>
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

                render <StackPanel>
                    foreach (var item in Items.Value) {
                        <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} OnToggle={OnToggle} />
                    }
                </StackPanel>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(card, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(emitted, "foreach (var item in");
        StringAssert.Contains(emitted, "Items.Value");
        StringAssert.Contains(emitted, "new ComponentNode(");
        StringAssert.Contains(emitted, "typeof(global::TestProject.TodoCardComponent)");
        StringAssert.Contains(emitted, "\"position0\"");
        StringAssert.Contains(emitted, "item.Id");
    }

    [TestMethod]
    public void Emit_TodoEditor_IncludesProjectedInputEventHints()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoEditor.csxaml",
            """
            component Element TodoEditor(string Title, bool IsDone, Action<string> OnTitleChanged, Action<bool> OnDoneChanged) {
                render <StackPanel>
                    <TextBox Text={Title} OnTextChanged={OnTitleChanged} />
                    <CheckBox IsChecked={IsDone} OnCheckedChanged={OnDoneChanged} />
                </StackPanel>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "\"OnTextChanged\"");
        StringAssert.Contains(emitted, "(global::System.Action<string>)(");
        StringAssert.Contains(emitted, "OnTitleChanged");
        StringAssert.Contains(emitted, "\"OnCheckedChanged\"");
        StringAssert.Contains(emitted, "(global::System.Action<bool>)(");
        StringAssert.Contains(emitted, "OnDoneChanged");
    }

    [TestMethod]
    public void Emit_BuiltInStyleExpression_UsesStyleValueKind()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <Button Content="Save" Style={TodoStyles.PrimaryButton} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "\"Style\"");
        StringAssert.Contains(emitted, "TodoStyles.PrimaryButton");
        StringAssert.Contains(emitted, "global::Csxaml.ControlMetadata.ValueKindHint.Style");
    }
}
