namespace Csxaml.Runtime;

public sealed record ExternalControlDescriptor(
    Type ControlType,
    Csxaml.ControlMetadata.ControlMetadata Metadata)
{
    public string TagName => Metadata.TagName;
}
