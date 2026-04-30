namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class SlotValidationTests
{
    [TestMethod]
    public void Validate_ComponentWithDefaultSlot_AllowsChildContent()
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

        GeneratorTestHarness.Validate(card, board);
    }

    [TestMethod]
    public void Validate_ComponentWithoutDefaultSlot_RejectsChildContent()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border />;
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

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(error.Message, "does not support child content");
    }

    [TestMethod]
    public void Validate_DuplicateDefaultSlots_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot />
                    <Slot />
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "more than one default slot");
    }

    [TestMethod]
    public void Validate_NamedSlot_AllowsNamedSlotDeclaration()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);

        var compilation = GeneratorTestHarness.Validate(component);
        var metadata = compilation.Components.GetAll().Single(component => component.Name == "TodoCard").Metadata;

        Assert.IsFalse(metadata.SupportsDefaultSlot);
        Assert.AreEqual("Header", metadata.NamedSlots.Single().Name);
    }

    [TestMethod]
    public void Validate_DuplicateNamedSlots_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot Name="Header" />
                    <Slot Name="Header" />
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "more than one named slot 'Header'");
    }

    [TestMethod]
    public void Validate_NamedSlotInsideForeach_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    foreach (var item in Items) {
                        <Slot Name="Header" />
                    }
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "named slot 'Header' cannot appear inside foreach");
    }

    [TestMethod]
    public void Validate_SlotWithGenericAttribute_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot Orientation="Horizontal" />
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "slot does not support attributes other than Name");
    }

    [TestMethod]
    public void Validate_SlotWithChildren_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot>
                        <TextBlock Text="Hello" />
                    </Slot>
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "slot does not support child content");
    }

    [TestMethod]
    public void Validate_RootSlot_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Slot />;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "default slot cannot be the component root");
    }

    [TestMethod]
    public void Validate_SlotInsideForeach_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    foreach (var item in Items) {
                        <Slot />
                    }
                </Border>;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Message, "default slot cannot appear inside foreach");
    }
}
