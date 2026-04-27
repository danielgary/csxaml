namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes a markup element reference discovered in CSXAML source text.
/// </summary>
/// <param name="TagName">The complete tag name text, including prefix when present.</param>
/// <param name="Prefix">The namespace prefix before the tag local name, or <see langword="null"/> when none is present.</param>
/// <param name="LocalName">The tag local name without a prefix.</param>
/// <param name="NameStart">The zero-based start offset of the tag name.</param>
/// <param name="NameLength">The length of the tag name.</param>
/// <param name="OpenTagStart">The zero-based start offset of the opening angle bracket.</param>
/// <param name="OpenTagEnd">The zero-based end offset of the opening tag.</param>
/// <param name="IsClosing">A value indicating whether the reference is a closing tag.</param>
/// <param name="Attributes">The attributes declared on the element reference.</param>
public sealed record CsxamlMarkupElementReference(
    string TagName,
    string? Prefix,
    string LocalName,
    int NameStart,
    int NameLength,
    int OpenTagStart,
    int OpenTagEnd,
    bool IsClosing,
    IReadOnlyList<CsxamlMarkupAttributeReference> Attributes);
