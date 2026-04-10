namespace Csxaml.Generator.Tests;

internal static class GeneratorTestHarness
{
    public static string Emit(params ParsedComponent[] components)
    {
        var catalog = new Validator().Validate(components);
        return new CodeEmitter().Emit(components[0].Definition, catalog);
    }

    public static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
    }

    public static ParsedComponent Parse(string path, string text)
    {
        var source = new SourceDocument(path, Normalize(text));
        var definition = new Parser().Parse(source);
        return new ParsedComponent(source, definition);
    }

    public static ComponentCatalog Validate(params ParsedComponent[] components)
    {
        return new Validator().Validate(components);
    }
}
