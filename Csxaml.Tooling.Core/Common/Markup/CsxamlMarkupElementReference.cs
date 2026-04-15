namespace Csxaml.Tooling.Core.Markup;

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
