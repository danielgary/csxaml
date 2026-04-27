namespace Csxaml.Runtime;

/// <summary>
/// Represents an event handler assignment on a native runtime node.
/// </summary>
/// <param name="Name">The event name exposed to CSXAML markup.</param>
/// <param name="Handler">The delegate that handles the event.</param>
/// <param name="ValueKindHint">A hint used when binding value-changing events.</param>
/// <param name="SourceInfo">Source-location metadata for the event assignment, when available.</param>
public sealed record NativeEventValue(
    string Name,
    Delegate Handler,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeEventValue"/> class with an unknown value kind.
    /// </summary>
    /// <param name="name">The event name exposed to CSXAML markup.</param>
    /// <param name="handler">The delegate that handles the event.</param>
    public NativeEventValue(string name, Delegate handler)
        : this(name, handler, ValueKindHint.Unknown, null)
    {
    }
}
