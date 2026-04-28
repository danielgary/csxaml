namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes the generated binding shape used for an event.
/// </summary>
public enum EventBindingKind
{
    /// <summary>
    /// The event is bound directly to a generated handler delegate.
    /// </summary>
    Direct,

    /// <summary>
    /// The event carries a text value change from a text-editing control.
    /// </summary>
    TextValueChanged,

    /// <summary>
    /// The event carries a Boolean value change from a toggle control.
    /// </summary>
    BoolValueChanged
}
