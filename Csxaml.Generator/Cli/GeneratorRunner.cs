namespace Csxaml.Generator;

internal sealed class GeneratorRunner
{
    private readonly CodeEmitter _codeEmitter = new();
    private readonly Parser _parser = new();
    private readonly Validator _validator = new();

    public IReadOnlyList<GeneratedFile> GenerateFiles(GeneratorOptions options)
    {
        var parsedComponents = options.InputFiles
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(ReadSource)
            .Select(source => new ParsedComponent(source, _parser.Parse(source)))
            .ToList();

        var catalog = _validator.Validate(parsedComponents);
        return parsedComponents
            .Select(component => CreateGeneratedFile(options.OutputDirectory, component, catalog))
            .ToList();
    }

    private static SourceDocument ReadSource(string path)
    {
        return new SourceDocument(path, File.ReadAllText(path));
    }

    private GeneratedFile CreateGeneratedFile(
        string outputDirectory,
        ParsedComponent component,
        ComponentCatalog catalog)
    {
        var outputPath = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(component.Source.FilePath) + ".g.cs");

        var content = _codeEmitter.Emit(component.Definition, catalog);
        return new GeneratedFile(outputPath, content);
    }
}
