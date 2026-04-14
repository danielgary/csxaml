namespace Csxaml.Generator;

internal sealed class CodeEmitter
{
    public string Emit(ParsedComponent component, CompilationContext compilation)
    {
        var writer = new IndentedCodeWriter();
        new ComponentEmitter(writer, compilation).Emit(component);
        return writer.ToString();
    }
}
