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
            .Select(component => CreateGeneratedFile(options.OutputDirectory, component, compilation))
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

    private GeneratedFile CreateGeneratedFile(
        string outputDirectory,
        ParsedComponent component,
        CompilationContext compilation)
    {
        var outputPath = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(component.Source.FilePath) + ".g.cs");

        var content = _codeEmitter.Emit(component, compilation);
        return new GeneratedFile(outputPath, content);
    }

    private static GeneratedFile CreateExternalRegistrationFile(
        string outputDirectory,
        CompilationContext compilation)
    {
        var writer = new IndentedCodeWriter();
        new GeneratedExternalControlRegistrationEmitter(writer).Emit(compilation);
        return new GeneratedFile(
            Path.Combine(outputDirectory, "GeneratedExternalControlRegistration.g.cs"),
            writer.ToString());
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
            writer.ToString());
    }
}
