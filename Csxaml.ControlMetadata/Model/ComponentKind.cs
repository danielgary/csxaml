namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes the source root kind used to generate a CSXAML component entry.
/// </summary>
public enum ComponentKind
{
    /// <summary>
    /// A normal retained CSXAML component.
    /// </summary>
    Element,

    /// <summary>
    /// A generated WinUI page root backed by a retained CSXAML body.
    /// </summary>
    Page,

    /// <summary>
    /// A generated WinUI window root backed by a retained CSXAML body.
    /// </summary>
    Window,

    /// <summary>
    /// A generated WinUI application root.
    /// </summary>
    Application,

    /// <summary>
    /// A generated WinUI resource dictionary root.
    /// </summary>
    ResourceDictionary
}
