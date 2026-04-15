namespace Csxaml.Generator;

internal static class GeneratedDiagnosticMapper
{
    public static Diagnostic? TryMap(
        GeneratedSourceMap map,
        int generatedStartLine,
        int generatedEndLine,
        string message)
    {
        var match = map.Entries
            .Where(entry =>
                generatedStartLine >= entry.GeneratedStartLine &&
                generatedEndLine <= entry.GeneratedEndLine)
            .OrderBy(entry => entry.GeneratedEndLine - entry.GeneratedStartLine)
            .FirstOrDefault();

        return match is null
            ? null
            : new Diagnostic(
                map.SourceFilePath,
                match.SourceStartLine,
                match.SourceStartColumn,
                match.SourceEndLine,
                match.SourceEndColumn,
                message);
    }
}
