namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes the concrete child-content shape for a native control.
/// </summary>
public enum ControlContentKind
{
    /// <summary>
    /// The control does not accept default child content.
    /// </summary>
    None,

    /// <summary>
    /// The control accepts one child through a property.
    /// </summary>
    Single,

    /// <summary>
    /// The control accepts many children through a collection property.
    /// </summary>
    Collection
}
