namespace Csxaml.Generator;

internal sealed record GeneratedSourceMap(
    string GeneratedFilePath,
    string SourceFilePath,
    string ComponentName,
    IReadOnlyList<GeneratedSourceMapEntry> Entries);
