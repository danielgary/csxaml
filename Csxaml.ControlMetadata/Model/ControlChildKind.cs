namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes how a native control accepts child content from CSXAML markup.
/// </summary>
public enum ControlChildKind
{
    /// <summary>
    /// The control does not accept child content.
    /// </summary>
    None,

    /// <summary>
    /// The control accepts exactly one child content node.
    /// </summary>
    Single,

    /// <summary>
    /// The control accepts multiple child content nodes.
    /// </summary>
    Multiple
}
