namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class ValidatorTests
{
    [TestMethod]
    public void Validate_MissingRequiredProp_ThrowsDiagnostic()
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
                render <StackPanel>
                    <TodoCard Title="One" IsDone={false} />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "missing required prop 'OnToggle'");
    }
}
