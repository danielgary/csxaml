namespace Csxaml.Generator;

internal sealed record FileScopedNamespaceDefinition(
    string NamespaceName,
    TextSpan Span);
