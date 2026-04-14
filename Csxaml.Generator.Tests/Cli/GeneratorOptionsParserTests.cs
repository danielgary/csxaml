namespace Csxaml.Generator.Tests.Cli;

[TestClass]
public sealed class GeneratorOptionsParserTests
{
    [TestMethod]
    public void Parse_AllowsOptionsBeforeInputsInAnySupportedOrder()
    {
        var options = GeneratorOptionsParser.Parse(
            new[]
            {
                "--assembly-name",
                "Demo.Assembly",
                "--out",
                "obj\\Csxaml\\Generated",
                "--generated-namespace",
                "Demo.Assembly.__CsxamlGenerated",
                "--references-file",
                "obj\\Csxaml\\References.txt",
                "--default-namespace",
                "Demo.Assembly",
                "First.csxaml",
                "Second.csxaml"
            });

        Assert.AreEqual("obj\\Csxaml\\Generated", options.OutputDirectory);
        Assert.AreEqual("Demo.Assembly", options.AssemblyName);
        Assert.AreEqual("Demo.Assembly", options.DefaultComponentNamespace);
        Assert.AreEqual("Demo.Assembly.__CsxamlGenerated", options.InternalGeneratedNamespace);
        CollectionAssert.AreEqual(
            new[] { "First.csxaml", "Second.csxaml" },
            options.InputFiles.ToArray());
    }
}
