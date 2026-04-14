namespace Csxaml.Generator;

internal sealed record ParsedComponent(
    SourceDocument Source,
    CsxamlFileDefinition File)
{
    public ComponentDefinition Definition => File.Component;
}
