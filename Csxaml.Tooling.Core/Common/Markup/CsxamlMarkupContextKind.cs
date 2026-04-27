namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes the kind of CSXAML markup context at an editor position.
/// </summary>
public enum CsxamlMarkupContextKind
{
    /// <summary>
    /// The position is not inside a supported markup completion context.
    /// </summary>
    None,

    /// <summary>
    /// The position is inside a tag name.
    /// </summary>
    TagName,

    /// <summary>
    /// The position is inside an attribute name.
    /// </summary>
    AttributeName,
}
