namespace Csxaml.Generator;

internal static class DiagnosticFactory
{
    public static DiagnosticException FromPosition(
        SourceDocument source,
        int position,
        string message)
    {
        var (line, column) = GetLineAndColumn(source.Text, position);
        return new DiagnosticException(new Diagnostic(source.FilePath, line, column, message));
    }

    public static DiagnosticException FromSpan(
        SourceDocument source,
        TextSpan span,
        string message)
    {
        return FromPosition(source, span.Start, message);
    }

    private static (int Line, int Column) GetLineAndColumn(string text, int position)
    {
        var line = 1;
        var column = 1;

        for (var index = 0; index < position && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }
}
