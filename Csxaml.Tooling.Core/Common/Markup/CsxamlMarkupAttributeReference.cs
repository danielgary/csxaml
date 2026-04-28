namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes an attribute name discovered on a CSXAML markup element.
/// </summary>
/// <param name="Name">The attribute name text.</param>
/// <param name="Start">The zero-based start offset of the attribute name.</param>
/// <param name="Length">The length of the attribute name.</param>
public sealed record CsxamlMarkupAttributeReference(
    string Name,
    int Start,
    int Length);
