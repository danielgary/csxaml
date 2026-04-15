namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlMarkupScanResult(
    IReadOnlyList<CsxamlUsingDirectiveInfo> UsingDirectives,
    CsxamlNamespaceDirectiveInfo? NamespaceDirective,
    IReadOnlyList<CsxamlComponentSignature> Components,
    IReadOnlyList<CsxamlMarkupElementReference> Elements);
