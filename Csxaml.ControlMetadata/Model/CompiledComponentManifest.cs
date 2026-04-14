namespace Csxaml.ControlMetadata;

public sealed record CompiledComponentManifest(
    IReadOnlyList<ComponentMetadata> Components);
