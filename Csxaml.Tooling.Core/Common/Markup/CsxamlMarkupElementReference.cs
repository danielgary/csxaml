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
/// <param name="PropertyContentOwner">The owner part of a property-content tag, when present.</param>
/// <param name="PropertyContentName">The property part of a property-content tag, when present.</param>
/// <param name="PropertyContentOwnerStart">The zero-based start offset of the owner part.</param>
/// <param name="PropertyContentOwnerLength">The length of the owner part.</param>
/// <param name="PropertyContentNameStart">The zero-based start offset of the property part.</param>
/// <param name="PropertyContentNameLength">The length of the property part.</param>
public sealed record CsxamlMarkupElementReference(
    string TagName,
    string? Prefix,
    string LocalName,
    int NameStart,
    int NameLength,
    int OpenTagStart,
    int OpenTagEnd,
    bool IsClosing,
    IReadOnlyList<CsxamlMarkupAttributeReference> Attributes,
    string? PropertyContentOwner = null,
    string? PropertyContentName = null,
    int PropertyContentOwnerStart = -1,
    int PropertyContentOwnerLength = 0,
    int PropertyContentNameStart = -1,
    int PropertyContentNameLength = 0);
