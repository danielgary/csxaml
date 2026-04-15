namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class SimpleTagResolutionTests
{
    [TestMethod]
    public void Validate_SimpleTagThatMatchesComponentAndBuiltInControl_ThrowsAmbiguityDiagnostic()
    {
        var button = GeneratorTestHarness.Parse(
            "Button.csxaml",
            """
            component Element Button {
                render <TextBlock Text="Local button component" />;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <Button />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(button, board));

        StringAssert.Contains(exception.Diagnostic.Message, "ambiguous");
        StringAssert.Contains(exception.Diagnostic.Message, "'Button'");
    }
}
