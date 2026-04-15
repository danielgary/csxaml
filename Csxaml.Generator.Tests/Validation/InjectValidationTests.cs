namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class InjectValidationTests
{
    [TestMethod]
    public void Validate_DuplicateInjectName_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                inject ITodoService todoService;
                inject IOtherService todoService;
                render <TextBlock Text="Todo" />;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Diagnostic.Message, "duplicate name 'todoService'");
    }

    [TestMethod]
    public void Validate_InjectNameCollisionWithParameter_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard(string title) {
                inject ITodoService title;
                render <TextBlock Text={title} />;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Diagnostic.Message, "duplicate name 'title'");
    }

    [TestMethod]
    public void Validate_InjectNameCollisionWithState_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                inject ITodoService count;
                State<int> count = new State<int>(0);
                render <TextBlock Text={count.Value} />;
            }
            """);

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(error.Diagnostic.Message, "duplicate name 'count'");
    }
}
