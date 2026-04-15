namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlMarkupContext(
    CsxamlMarkupContextKind Kind,
    string PrefixText,
    string? Qualifier,
    string? TagName,
    IReadOnlyList<string> ExistingAttributeNames);
