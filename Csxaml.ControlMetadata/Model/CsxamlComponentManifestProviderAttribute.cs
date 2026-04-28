namespace Csxaml.ControlMetadata;

/// <summary>
/// Identifies the type that supplies compiled CSXAML component metadata for an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class CsxamlComponentManifestProviderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsxamlComponentManifestProviderAttribute"/> class.
    /// </summary>
    /// <param name="providerType">The provider type that implements <see cref="IComponentManifestProvider"/>.</param>
    public CsxamlComponentManifestProviderAttribute(Type providerType)
    {
        ProviderType = providerType;
    }

    /// <summary>
    /// Gets the provider type that returns the compiled component manifest.
    /// </summary>
    public Type ProviderType { get; }
}
