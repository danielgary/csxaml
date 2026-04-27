namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes the markup context at an editor cursor position.
/// </summary>
/// <param name="Kind">The kind of markup context at the position.</param>
/// <param name="PrefixText">The partially typed text for the current tag or attribute.</param>
/// <param name="Qualifier">The namespace or alias qualifier for a tag name, when present.</param>
/// <param name="TagName">The current tag name when completing attributes.</param>
/// <param name="ExistingAttributeNames">Attribute names already present on the current tag.</param>
public sealed record CsxamlMarkupContext(
    CsxamlMarkupContextKind Kind,
    string PrefixText,
    string? Qualifier,
    string? TagName,
    IReadOnlyList<string> ExistingAttributeNames);
