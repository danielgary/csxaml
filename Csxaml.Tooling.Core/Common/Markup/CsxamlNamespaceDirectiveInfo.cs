namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlNamespaceDirectiveInfo(
    string NamespaceName,
    int Start,
    int Length);
