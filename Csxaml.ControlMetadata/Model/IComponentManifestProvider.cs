namespace Csxaml.ControlMetadata;

/// <summary>
/// Provides metadata for CSXAML components compiled into an assembly.
/// </summary>
public interface IComponentManifestProvider
{
    /// <summary>
    /// Gets the compiled component manifest for the assembly.
    /// </summary>
    /// <returns>The component manifest exposed by the assembly.</returns>
    CompiledComponentManifest GetManifest();
}
