namespace Csxaml.Generator;

internal sealed record GeneratedCodeDocument(
    string Text,
    IReadOnlyList<GeneratedSourceMapEntry> SourceMapEntries);
