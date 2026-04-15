using Microsoft.CodeAnalysis;
using RoslynDiagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace Csxaml.Generator.Tests.Diagnostics;

[TestClass]
public sealed class BuildDiagnosticMappingTests
{
    [TestMethod]
    public void Compile_HelperCodeError_MapsToCsxamlSource()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                string BuildTitle()
                {
                    return MissingSymbol;
                }

                render <TextBlock Text={BuildTitle()} />;
            }
            """);

        var diagnostic = Compile(component, "CS0103");
        var mappedSpan = diagnostic.Location.GetMappedLineSpan();

        Assert.AreEqual("TodoBoard.csxaml", mappedSpan.Path);
        Assert.AreEqual(4, mappedSpan.StartLinePosition.Line + 1);
        StringAssert.Contains(diagnostic.GetMessage(), "MissingSymbol");
    }

    [TestMethod]
    public void Compile_ExpressionIslandError_MapsToCsxamlSource()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <TextBlock Text={MissingSymbol} />;
            }
            """);

        var diagnostic = Compile(component, "CS0103");
        var mappedSpan = diagnostic.Location.GetMappedLineSpan();

        Assert.AreEqual("TodoBoard.csxaml", mappedSpan.Path);
        Assert.AreEqual(2, mappedSpan.StartLinePosition.Line + 1);
        StringAssert.Contains(diagnostic.GetMessage(), "MissingSymbol");
    }

    private static RoslynDiagnostic Compile(ParsedComponent component, string diagnosticId)
    {
        var diagnostics = GeneratedCompilationTestHarness.Compile(GeneratorTestHarness.Emit(component));
        return diagnostics.Single(diagnostic => diagnostic.Id == diagnosticId);
    }
}
