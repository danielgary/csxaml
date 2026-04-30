namespace Csxaml.Generator;

internal static class GeneratedExpressionFormatter
{
    public static string FormatArgumentList(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>(arguments.Count);
        for (var index = 0; index < arguments.Count; index++)
        {
            lines.Add(
                index == arguments.Count - 1
                    ? CodeBlockFormatter.FormatLastArgument(arguments[index], 4)
                    : CodeBlockFormatter.FormatArgument(arguments[index], 4));
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatTypeLiteral(string clrTypeName)
    {
        return $"global::{clrTypeName.Replace("+", ".", StringComparison.Ordinal)}";
    }
}
