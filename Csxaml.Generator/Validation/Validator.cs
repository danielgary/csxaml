namespace Csxaml.Generator;

internal sealed class Validator
{
    private readonly ComponentDefinitionValidator _componentDefinitionValidator = new();
    private readonly ReferencedComponentCatalogBuilder _referencedComponentCatalogBuilder = new();
    private readonly ExternalControlCatalogBuilder _externalControlCatalogBuilder = new();

    public CompilationContext Validate(IReadOnlyList<ParsedComponent> components)
    {
        return Validate(components, ProjectGenerationContext.CreateDefault(), Array.Empty<string>());
    }

    public CompilationContext Validate(
        IReadOnlyList<ParsedComponent> components,
        IReadOnlyList<string> referencePaths)
    {
        return Validate(components, ProjectGenerationContext.CreateDefault(), referencePaths);
    }

    public CompilationContext Validate(
        IReadOnlyList<ParsedComponent> components,
        ProjectGenerationContext? project = null,
        IReadOnlyList<string>? referencePaths = null)
    {
        var projectContext = project ?? ProjectGenerationContext.CreateDefault();
        var referencedComponents = _referencedComponentCatalogBuilder.Build(referencePaths ?? Array.Empty<string>());
        var componentCatalog = ComponentCatalogBuilder.Build(components, projectContext, referencedComponents);
        var nativeControls = _externalControlCatalogBuilder.Build(
            components,
            referencePaths ?? Array.Empty<string>());
        var compilation = new CompilationContext(projectContext, componentCatalog, nativeControls);
        foreach (var component in components)
        {
            _componentDefinitionValidator.Validate(component, compilation);
        }

        return compilation;
    }
}
