namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class NativeValidationTests
{
    [TestMethod]
    public void Validate_InvalidNativeEvent_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <StackPanel>
                    <TextBlock Text="Todo Board" OnClick={Toggle} />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown attribute 'OnClick' on native control 'TextBlock'");
    }

    [TestMethod]
    public void Validate_InvalidNativeProp_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <StackPanel>
                    <Button Content="Toggle" NotAProp={true} OnClick={Toggle} />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown attribute 'NotAProp' on native control 'Button'");
    }

    [TestMethod]
    public void Validate_StringLiteralEventValue_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <StackPanel>
                    <Button Content="Toggle" OnClick="Toggle" />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "native event 'OnClick' on 'Button' requires an expression value");
    }
}
