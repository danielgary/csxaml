using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

public static partial class CsxamlUsingDirectiveScanner
{
    public static IReadOnlyList<CsxamlUsingDirectiveInfo> Scan(string text)
    {
        var directives = new List<CsxamlUsingDirectiveInfo>();
        foreach (Match match in UsingDirectivePattern().Matches(text))
        {
            var aliasGroup = match.Groups["alias"];
            var qualifiedNameGroup = match.Groups["qualified"];
            var isStatic = match.Groups["static"].Success;
            directives.Add(
                new CsxamlUsingDirectiveInfo(
                    qualifiedNameGroup.Value,
                    aliasGroup.Success ? aliasGroup.Value : null,
                    isStatic,
                    match.Index,
                    match.Length));
        }

        return directives;
    }

    [GeneratedRegex(
        @"^\s*using\s+(?:(?<static>static)\s+)?(?:(?<alias>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*)?(?<qualified>[A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex UsingDirectivePattern();
}
