namespace Csxaml.Tooling.Core.Hover;

internal static class CsxamlKeywordHoverFormatter
{
    public static string FormatForeachKeyword()
    {
        var lines = new List<string>
        {
            "```csxaml",
            "foreach (var item in Items) { ... }",
            "```",
            "Repeated child rendering",
            string.Empty,
            "- Renders one CSXAML child subtree per item",
            "- Use stable `Key` values when item identity matters",
            "- This is not virtualization; use a native virtualized control for very large scrolling item surfaces",
        };

        return string.Join(Environment.NewLine, lines);
    }
}
