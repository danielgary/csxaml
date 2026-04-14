namespace Csxaml.Generator;

internal sealed record CsxamlFileDefinition(
    IReadOnlyList<UsingDirectiveDefinition> UsingDirectives,
    FileScopedNamespaceDefinition? Namespace,
    IReadOnlyList<FileHelperCodeBlock> HelperCodeBlocks,
    ComponentDefinition Component,
    TextSpan Span);
