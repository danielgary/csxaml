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
                render <StackPanel>
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
                render <Border>
                    <Slot />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <TodoCard>
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

    [TestMethod]
    public void Emit_NamedSlotOutlet_RendersNamedSlotContent()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <StackPanel>
                    <Slot Name="Header" />
                </StackPanel>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "AddRange(GetNamedSlotContent(\"Header\"));");
    }

    [TestMethod]
    public void Emit_ComponentUsageWithNamedSlot_BuildsNamedSlotContent()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <TodoCard>
                    <TodoCard.Header>
                        <TextBlock Text="Hello" />
                    </TodoCard.Header>
                </TodoCard>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(card, board);
        var emitted = new CodeEmitter().Emit(board, compilation);

        StringAssert.Contains(emitted, "var namedSlots");
        StringAssert.Contains(emitted, "[\"Header\"]");
        StringAssert.Contains(emitted, "new ComponentNode(");
    }

    [TestMethod]
    public void Emit_Manifest_IncludesNamedSlotMetadata()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(card);
        var writer = new IndentedCodeWriter();
        new GeneratedComponentManifestEmitter(writer).Emit(
            compilation,
            compilation.Components.FindLocalComponents());
        var emitted = writer.ToString();

        StringAssert.Contains(emitted, "new ComponentSlotMetadata(\"Header\")");
    }
}
