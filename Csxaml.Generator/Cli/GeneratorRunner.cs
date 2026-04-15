namespace Csxaml.Generator;

internal sealed class GeneratorRunner
{
    private readonly CodeEmitter _codeEmitter = new();
    private readonly Parser _parser = new();
    private readonly Validator _validator = new();

    public IReadOnlyList<GeneratedFile> GenerateFiles(GeneratorOptions options)
    {
        var project = new ProjectGenerationContext(
            options.AssemblyName,
            options.DefaultComponentNamespace,
            options.InternalGeneratedNamespace);
        var parsedComponents = options.InputFiles
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(ReadSource)
            .Select(source => new ParsedComponent(source, _parser.Parse(source)))
            .ToList();

        var compilation = _validator.Validate(parsedComponents, project, options.ReferencePaths);
        var files = parsedComponents
            .SelectMany(component => CreateGeneratedFiles(options.OutputDirectory, component, compilation))
            .ToList();
        files.Add(CreateComponentManifestFile(options.OutputDirectory, compilation));
        if (compilation.HasExternalControls)
        {
            files.Add(CreateExternalRegistrationFile(options.OutputDirectory, compilation));
        }

        return files;
    }

    private static SourceDocument ReadSource(string path)
    {
        return new SourceDocument(path, File.ReadAllText(path));
    }

    private IReadOnlyList<GeneratedFile> CreateGeneratedFiles(
        string outputDirectory,
        ParsedComponent component,
        CompilationContext compilation)
    {
        var outputPath = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(component.Source.FilePath) + ".g.cs");
        var document = _codeEmitter.EmitDocument(component, compilation);
        var mapPath = Path.Combine(
            Directory.GetParent(outputDirectory)?.FullName ?? outputDirectory,
            "Maps",
            Path.GetFileNameWithoutExtension(component.Source.FilePath) + ".map.json");
        var map = new GeneratedSourceMap(
            outputPath,
            component.Source.FilePath,
            component.Definition.Name,
            document.SourceMapEntries);

        return
        [
            new GeneratedFile(outputPath, document.Text, outputDirectory),
            new GeneratedFile(
                mapPath,
                GeneratedSourceMapWriter.Write(map),
                Path.Combine(Directory.GetParent(outputDirectory)?.FullName ?? outputDirectory, "Maps"))
        ];
    }

    private static GeneratedFile CreateExternalRegistrationFile(
        string outputDirectory,
        CompilationContext compilation)
    {
        var writer = new IndentedCodeWriter();
        new GeneratedExternalControlRegistrationEmitter(writer).Emit(compilation);
        return new GeneratedFile(
            Path.Combine(outputDirectory, "GeneratedExternalControlRegistration.g.cs"),
            writer.ToString(),
            outputDirectory);
    }

    private static GeneratedFile CreateComponentManifestFile(
        string outputDirectory,
        CompilationContext compilation)
    {
        var writer = new IndentedCodeWriter();
        new GeneratedComponentManifestEmitter(writer).Emit(
            compilation,
            compilation.Components.FindLocalComponents());
        return new GeneratedFile(
            Path.Combine(outputDirectory, "GeneratedComponentManifest.g.cs"),
            writer.ToString(),
            outputDirectory);
    }
}
