namespace Csxaml.ControlMetadata;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class CsxamlComponentManifestProviderAttribute : Attribute
{
    public CsxamlComponentManifestProviderAttribute(Type providerType)
    {
        ProviderType = providerType;
    }

    public Type ProviderType { get; }
}
