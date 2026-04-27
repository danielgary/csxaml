namespace Csxaml.Runtime;

/// <summary>
/// Describes a native control type that can be registered for CSXAML rendering.
/// </summary>
/// <param name="ControlType">The CLR type created when the external control tag is rendered.</param>
/// <param name="Metadata">The control metadata used by the runtime and tooling.</param>
public sealed record ExternalControlDescriptor(
    Type ControlType,
    Csxaml.ControlMetadata.ControlMetadata Metadata)
{
    /// <summary>
    /// Gets the CSXAML tag name for the external control.
    /// </summary>
    public string TagName => Metadata.TagName;
}
