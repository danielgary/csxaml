namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class SlotEmissionTests
{
    [TestMethod]
    public void Emit_DefaultSlotOutlet_RendersChildContent()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                return <StackPanel>
                    <TextBlock Text="Header" />
                    <Slot />
                </StackPanel>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "AddRange(ChildContent);");
    }

    [TestMethod]
    public void Emit_ComponentUsageWithChildren_BuildsChildContentList()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                return <Border>
                    <Slot />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <TodoCard>
                    <TextBlock Text="Hello" />
                </TodoCard>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(card, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(emitted, "var childContent0 = new List<Node>();");
        StringAssert.Contains(emitted, "new ComponentNode(");
        StringAssert.Contains(emitted, "typeof(global::TestProject.TodoCardComponent)");
        StringAssert.Contains(emitted, "childContent0");
    }
}
