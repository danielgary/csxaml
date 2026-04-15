namespace Csxaml.Generator;

internal sealed class CodeEmitter
{
    public string Emit(ParsedComponent component, CompilationContext compilation)
    {
        return EmitDocument(component, compilation).Text;
    }

    public GeneratedCodeDocument EmitDocument(ParsedComponent component, CompilationContext compilation)
    {
        var writer = new IndentedCodeWriter();
        new ComponentEmitter(writer, compilation).Emit(component);
        return new GeneratedCodeDocument(writer.ToString(), writer.SourceMapEntries.ToList());
    }
}
