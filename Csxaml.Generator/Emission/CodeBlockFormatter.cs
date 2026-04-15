namespace Csxaml.Generator;

internal static class CodeBlockFormatter
{
    public static string FormatArgument(string text, int indentSpaces = 4)
    {
        return FormatEntry(text, indentSpaces, trailingComma: true);
    }

    public static string FormatLastArgument(string text, int indentSpaces = 4)
    {
        return Indent(text, indentSpaces);
    }

    public static string FormatListItem(string text, int indentSpaces = 4)
    {
        return FormatEntry(text, indentSpaces, trailingComma: true);
    }

    public static string Indent(string text, int indentSpaces)
    {
        var padding = new string(' ', indentSpaces);
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        return string.Join(
            Environment.NewLine,
            normalized.Split('\n').Select(line => $"{padding}{line}"));
    }

    private static string FormatEntry(string text, int indentSpaces, bool trailingComma)
    {
        var indented = Indent(text, indentSpaces);
        return trailingComma
            ? $"{indented}{Environment.NewLine}{new string(' ', indentSpaces)},"
            : indented;
    }
}
