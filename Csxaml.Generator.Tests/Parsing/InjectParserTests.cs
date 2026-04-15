namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class InjectParserTests
{
    [TestMethod]
    public void Parse_ComponentPrologue_PreservesInjectDeclarations()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                inject ITodoService todoService;
                State<int> Count = new State<int>(LoadCount());
                inject ILogger<TodoBoard> logger;

                render <TextBlock Text={Count.Value} />;
            }
            """).Definition;

        Assert.HasCount(2, component.InjectFields);
        Assert.AreEqual("ITodoService", component.InjectFields[0].TypeName);
        Assert.AreEqual("todoService", component.InjectFields[0].Name);
        Assert.AreEqual("ILogger<TodoBoard>", component.InjectFields[1].TypeName);
        Assert.AreEqual("logger", component.InjectFields[1].Name);
        Assert.HasCount(1, component.StateFields);
        Assert.AreEqual("Count", component.StateFields[0].Name);
    }

    [TestMethod]
    public void Parse_InvalidInjectDeclaration_ThrowsDiagnostic()
    {
        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse(
                "TodoBoard.csxaml",
                """
                component Element TodoBoard {
                    inject ;
                    render <TextBlock Text="Todo" />;
                }
                """));

        StringAssert.Contains(error.Diagnostic.Message, "invalid inject declaration");
    }

    [TestMethod]
    public void Parse_MisplacedInjectAfterHelperCode_ThrowsDiagnostic()
    {
        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse(
                "TodoBoard.csxaml",
                """
                component Element TodoBoard {
                    string BuildTitle() => "Todo";
                    inject ITodoService todoService;

                    render <TextBlock Text={BuildTitle()} />;
                }
                """));

        StringAssert.Contains(error.Diagnostic.Message, "inject declarations must appear before helper code");
    }
}
