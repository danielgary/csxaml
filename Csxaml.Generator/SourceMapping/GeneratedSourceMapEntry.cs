namespace Csxaml.Generator;

internal sealed record GeneratedSourceMapEntry(
    string Kind,
    string? Label,
    int GeneratedStartLine,
    int GeneratedEndLine,
    int SourceStartLine,
    int SourceStartColumn,
    int SourceEndLine,
    int SourceEndColumn);
