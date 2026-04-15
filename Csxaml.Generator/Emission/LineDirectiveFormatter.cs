namespace Csxaml.Generator;

internal static class LineDirectiveFormatter
{
    public static string Wrap(SourceDocument source, TextSpan span, string text)
    {
        var start = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, span.Start);
        return
            $"""
            #line {start.Line} "{EscapeFilePath(source.FilePath)}"
            {text}
            #line default
            #line hidden
            """;
    }

    private static string EscapeFilePath(string filePath)
    {
        return filePath
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
