namespace Csxaml.ControlMetadata;

/// <summary>
/// Classifies values so tooling and emitters can choose the correct conversion behavior.
/// </summary>
public enum ValueKindHint
{
    /// <summary>
    /// The value should be treated as a string.
    /// </summary>
    String,

    /// <summary>
    /// The value should be treated as a Boolean.
    /// </summary>
    Bool,

    /// <summary>
    /// The value should be treated as an integer.
    /// </summary>
    Int,

    /// <summary>
    /// The value should be treated as a floating-point number.
    /// </summary>
    Double,

    /// <summary>
    /// The value should be treated as an enum member.
    /// </summary>
    Enum,

    /// <summary>
    /// The value should be treated as an object expression.
    /// </summary>
    Object,

    /// <summary>
    /// The value should be treated as a brush value.
    /// </summary>
    Brush,

    /// <summary>
    /// The value should be treated as a thickness value.
    /// </summary>
    Thickness,

    /// <summary>
    /// The value should be treated as a style object.
    /// </summary>
    Style,

    /// <summary>
    /// No specific conversion hint is known.
    /// </summary>
    Unknown
}
