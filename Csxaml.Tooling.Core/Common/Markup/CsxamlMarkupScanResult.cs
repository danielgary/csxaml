namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Contains source references discovered by scanning CSXAML text.
/// </summary>
/// <param name="UsingDirectives">The using directives declared in the source text.</param>
/// <param name="NamespaceDirective">The namespace directive declared in the source text, when present.</param>
/// <param name="Components">The component declarations discovered in the source text.</param>
/// <param name="Elements">The markup element references discovered in the source text.</param>
public sealed record CsxamlMarkupScanResult(
    IReadOnlyList<CsxamlUsingDirectiveInfo> UsingDirectives,
    CsxamlNamespaceDirectiveInfo? NamespaceDirective,
    IReadOnlyList<CsxamlComponentSignature> Components,
    IReadOnlyList<CsxamlMarkupElementReference> Elements);
