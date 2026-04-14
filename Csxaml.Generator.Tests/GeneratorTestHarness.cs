namespace Csxaml.Generator.Tests;

internal static class GeneratorTestHarness
{
    public static ProjectGenerationContext CreateProjectContext(
        string assemblyName = "TestProject",
        string defaultNamespace = "TestProject")
    {
        return new ProjectGenerationContext(
            assemblyName,
            defaultNamespace,
            $"{defaultNamespace}.__CsxamlGenerated");
    }

    public static string Emit(params ParsedComponent[] components)
    {
        var compilation = new Validator().Validate(components, CreateProjectContext());
        return new CodeEmitter().Emit(components[0], compilation);
    }

    public static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
    }

    public static ParsedComponent Parse(string path, string text)
    {
        var source = new SourceDocument(path, Normalize(text));
        var file = new Parser().Parse(source);
        return new ParsedComponent(source, file);
    }

    public static CompilationContext Validate(params ParsedComponent[] components)
    {
        return new Validator().Validate(components, CreateProjectContext());
    }

    public static CompilationContext ValidateWithReferences(
        IReadOnlyList<string> referencePaths,
        params ParsedComponent[] components)
    {
        return new Validator().Validate(components, CreateProjectContext(), referencePaths);
    }
}
