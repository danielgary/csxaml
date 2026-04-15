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
            var namespaceGroup = match.Groups["namespace"];
            directives.Add(
                new CsxamlUsingDirectiveInfo(
                    namespaceGroup.Value,
                    aliasGroup.Success ? aliasGroup.Value : null,
                    match.Index,
                    match.Length));
        }

        return directives;
    }

    [GeneratedRegex(
        @"^\s*using\s+(?:(?<alias>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*)?(?<namespace>[A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex UsingDirectivePattern();
}
