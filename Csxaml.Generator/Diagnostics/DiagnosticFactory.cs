namespace Csxaml.Generator;

internal static class DiagnosticFactory
{
    public static DiagnosticException FromPosition(
        SourceDocument source,
        int position,
        string message)
    {
        var coordinate = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, position);
        return new DiagnosticException(
            new Diagnostic(
                source.FilePath,
                coordinate.Line,
                coordinate.Column,
                coordinate.Line,
                coordinate.Column + 1,
                message));
    }

    public static DiagnosticException FromSpan(
        SourceDocument source,
        TextSpan span,
        string message)
    {
        var safeEnd = span.Length == 0 ? span.Start : Math.Max(span.Start, span.End - 1);
        var start = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, span.Start);
        var end = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, safeEnd);
        return new DiagnosticException(
            new Diagnostic(
                source.FilePath,
                start.Line,
                start.Column,
                end.Line,
                end.Column + 1,
                message));
    }
}
