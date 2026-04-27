namespace Csxaml.ControlMetadata;

/// <summary>
/// Lists the CSXAML components compiled into an assembly.
/// </summary>
/// <param name="Components">The compiled component metadata entries available from the assembly.</param>
public sealed record CompiledComponentManifest(
    IReadOnlyList<ComponentMetadata> Components);
