namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class NamedSlotValidationTests
{
    [TestMethod]
    public void Validate_ComponentNamedSlot_AllowsPropertyContent()
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

        GeneratorTestHarness.Validate(card, board);
    }

    [TestMethod]
    public void Validate_ComponentNamedSlot_RejectsUnknownSlot()
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
                    <TodoCard.Footer>
                        <TextBlock Text="Hello" />
                    </TodoCard.Footer>
                </TodoCard>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(error.Message, "unknown named slot 'Footer' on component 'TodoCard'");
    }

    [TestMethod]
    public void Validate_ComponentNamedSlot_RejectsDuplicateContentBlocks()
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
                        <TextBlock Text="One" />
                    </TodoCard.Header>
                    <TodoCard.Header>
                        <TextBlock Text="Two" />
                    </TodoCard.Header>
                </TodoCard>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(error.Message, "named slot 'Header' on component 'TodoCard' is assigned more than once");
    }

    [TestMethod]
    public void Validate_ComponentNamedSlot_RejectsPropCollision()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Header) {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <TodoCard Header="Prop">
                    <TodoCard.Header>
                        <TextBlock Text="Slot" />
                    </TodoCard.Header>
                </TodoCard>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(error.Message, "named slot 'Header' on component 'TodoCard' collides with a prop assignment");
    }
}
