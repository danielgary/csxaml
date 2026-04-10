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
                return <StackPanel>
                    <TextBlock Text={Title} />
                    if (IsDone) {
                        <TextBlock Text="Done" />
                    }
                    <Button Content="Toggle" OnClick={OnToggle} />
                </StackPanel>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);
        const string expected = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Csxaml.Runtime;

            namespace GeneratedCsxaml;

            public sealed record TodoCardProps(string Title, bool IsDone, Action OnToggle);

            public sealed class TodoCardComponent : ComponentInstance<TodoCardProps>
            {
                private string Title => Props.Title;
                private bool IsDone => Props.IsDone;
                private Action OnToggle => Props.OnToggle;

                public override Node Render()
                {
                    var children = new List<Node>();
                    children.Add(new TextBlockNode(Title));
                    if (IsDone)
                    {
                        children.Add(new TextBlockNode("Done"));
                    }
                    children.Add(new ButtonNode("Toggle", OnToggle));
                    return new StackPanelNode(children);
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

        var catalog = GeneratorTestHarness.Validate(card, board);
        var emitted = new CodeEmitter().Emit(board.Definition, catalog);

        StringAssert.Contains(emitted, "foreach (var item in Items.Value)");
        StringAssert.Contains(emitted, "new ComponentNode(typeof(TodoCardComponent)");
        StringAssert.Contains(emitted, "\"slot0\", item.Id");
    }
}
