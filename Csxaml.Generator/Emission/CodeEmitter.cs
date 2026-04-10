namespace Csxaml.Generator;

internal sealed class CodeEmitter
{
    public string Emit(ComponentDefinition component, ComponentCatalog catalog)
    {
        var writer = new IndentedCodeWriter();
        new ComponentEmitter(writer, catalog).Emit(component);
        return writer.ToString();
    }
}
