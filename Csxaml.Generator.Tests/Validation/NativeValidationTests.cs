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

    [TestMethod]
    public void Validate_TextBoxAndCheckBoxProjectedEvents_AreRecognized()
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

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_BuiltInStyleExpression_IsRecognized()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                return <StackPanel>
                    <Button Content="Save" Style={TodoStyles.PrimaryButton} />
                </StackPanel>;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_TextBoxStringLiteralEventValue_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoEditor.csxaml",
            """
            component Element TodoEditor {
                return <StackPanel>
                    <TextBox Text="Draft" OnTextChanged="ChangeTitle" />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "native event 'OnTextChanged' on 'TextBox' requires an expression value");
    }
}
