namespace Csxaml.Generator;

internal sealed class CompilationContext
{
    public CompilationContext(
        ComponentCatalog components,
        NativeControlCatalog nativeControls)
        : this(ProjectGenerationContext.CreateDefault(), components, nativeControls)
    {
    }

    public CompilationContext(
        ProjectGenerationContext project,
        ComponentCatalog components,
        NativeControlCatalog nativeControls)
    {
        Project = project;
        Components = components;
        NativeControls = nativeControls;
    }

    public ComponentCatalog Components { get; }

    public bool HasExternalControls => NativeControls.ExternalControls.Count > 0;

    public NativeControlCatalog NativeControls { get; }

    public ProjectGenerationContext Project { get; }
}
