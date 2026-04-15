namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class UsingDirectiveParserTests
{
    [TestMethod]
    public void Parse_FileLevelUsingDirectives_ProducesUsingDefinitions()
    {
        const string sourceText = """
            using Microsoft.UI.Xaml.Controls;
            using WinUi = Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <StackPanel />;
            }
            """;

        var file = new Parser().Parse(
            new SourceDocument("TodoBoard.csxaml", GeneratorTestHarness.Normalize(sourceText)));

        Assert.HasCount(2, file.UsingDirectives);
        Assert.AreEqual("Microsoft.UI.Xaml.Controls", file.UsingDirectives[0].QualifiedName);
        Assert.IsNull(file.UsingDirectives[0].Alias);
        Assert.IsFalse(file.UsingDirectives[0].IsStatic);
        Assert.AreEqual("WinUi", file.UsingDirectives[1].Alias);
        Assert.AreEqual("Microsoft.UI.Xaml.Controls", file.UsingDirectives[1].QualifiedName);
    }

    [TestMethod]
    public void Parse_StaticUsingDirective_PreservesStaticImport()
    {
        const string sourceText = """
            using static System.Math;

            component Element TodoBoard {
                render <TextBlock Text={Max(1, 2).ToString()} />;
            }
            """;

        var file = new Parser().Parse(
            new SourceDocument("TodoBoard.csxaml", GeneratorTestHarness.Normalize(sourceText)));

        Assert.HasCount(1, file.UsingDirectives);
        Assert.AreEqual("System.Math", file.UsingDirectives[0].QualifiedName);
        Assert.IsNull(file.UsingDirectives[0].Alias);
        Assert.IsTrue(file.UsingDirectives[0].IsStatic);
    }
}
