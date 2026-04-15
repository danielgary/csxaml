namespace Csxaml.Generator;

internal static class SourceInfoEmitter
{
    public static string Emit(
        SourceDocument source,
        string componentName,
        TextSpan span,
        string? tagName = null,
        string? memberName = null)
    {
        var safeEnd = span.Length == 0 ? span.Start : Math.Max(span.Start, span.End - 1);
        var start = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, span.Start);
        var end = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, safeEnd);
        return
            $"new global::Csxaml.Runtime.CsxamlSourceInfo(\"{Escape(source.FilePath)}\", {start.Line}, {start.Column}, {end.Line}, {end.Column + 1}, \"{Escape(componentName)}\", {FormatNullable(tagName)}, {FormatNullable(memberName)})";
    }

    private static string FormatNullable(string? value)
    {
        return value is null
            ? "null"
            : $"\"{Escape(value)}\"";
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
