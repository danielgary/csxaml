namespace Csxaml.Generator.Tests.Diagnostics;

[TestClass]
public sealed class SourceMappingTests
{
    [TestMethod]
    public void EmitDocument_RecordsSourceMapEntriesForRelevantRegions()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone) {
                return <Border>
                    if (IsDone) {
                        <TextBlock Text={Title} />
                    }
                </Border>;
            }
            """);
        var document = new CodeEmitter().EmitDocument(component, GeneratorTestHarness.Validate(component));
        var map = new GeneratedSourceMap(
            @"obj\Debug\net10.0\Csxaml\Generated\TodoCard.g.cs",
            component.Source.FilePath,
            component.Definition.Name,
            document.SourceMapEntries);
        var json = GeneratedSourceMapWriter.Write(map);

        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "props-record" && entry.Label == "TodoCard"));
        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "component-parameter" && entry.Label == "Title"));
        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "if-block" && entry.Label == "if"));
        Assert.IsTrue(document.SourceMapEntries.Any(entry => entry.Kind == "native-tag" && entry.Label == "TextBlock"));
        StringAssert.Contains(json, "\"ComponentName\": \"TodoCard\"");
        StringAssert.Contains(json, "\"Kind\": \"if-block\"");
    }

    [TestMethod]
    public void GeneratedDiagnosticMapper_MapsGeneratedLinesBackToSourceSpan()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title) {
                return <TextBlock Text={Title} />;
            }
            """);
        var document = new CodeEmitter().EmitDocument(component, GeneratorTestHarness.Validate(component));
        var entry = document.SourceMapEntries.Single(sourceMapEntry =>
            sourceMapEntry.Kind == "native-tag" &&
            sourceMapEntry.Label == "TextBlock");
        var map = new GeneratedSourceMap(
            @"obj\Debug\net10.0\Csxaml\Generated\TodoCard.g.cs",
            component.Source.FilePath,
            component.Definition.Name,
            document.SourceMapEntries);

        var diagnostic = GeneratedDiagnosticMapper.TryMap(
            map,
            entry.GeneratedStartLine,
            entry.GeneratedEndLine,
            "generated failure");

        Assert.IsNotNull(diagnostic);
        Assert.AreEqual("TodoCard.csxaml", diagnostic.FilePath);
        Assert.AreEqual(entry.SourceStartLine, diagnostic.Line);
        Assert.AreEqual(entry.SourceEndColumn, diagnostic.EndColumn);
    }

    [TestMethod]
    public void GeneratorRunner_WritesMapSidecarsUnderIntermediateMapsDirectory()
    {
        var rootDirectory = CreateRootDirectory();

        try
        {
            var sourcePath = Path.Combine(rootDirectory, "TodoBoard.csxaml");
            File.WriteAllText(
                sourcePath,
                GeneratorTestHarness.Normalize(
                    """
                    component Element TodoBoard {
                        return <TextBlock Text="Todo" />;
                    }
                    """));

            var outputDirectory = Path.Combine(rootDirectory, "obj", "Debug", "net10.0", "Csxaml", "Generated");
            var files = new GeneratorRunner().GenerateFiles(
                new GeneratorOptions(
                    outputDirectory,
                    "TestProject",
                    "TestProject",
                    "TestProject.__CsxamlGenerated",
                    Array.Empty<string>(),
                    [sourcePath]));
            OutputWriter.WriteAll(outputDirectory, files);

            var mapPath = Path.Combine(rootDirectory, "obj", "Debug", "net10.0", "Csxaml", "Maps", "TodoBoard.map.json");
            Assert.IsTrue(files.Any(file => string.Equals(file.OutputPath, mapPath, StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(File.Exists(mapPath));
            StringAssert.Contains(File.ReadAllText(mapPath), "\"ComponentName\": \"TodoBoard\"");
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private static string CreateRootDirectory()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "SourceMappingTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
