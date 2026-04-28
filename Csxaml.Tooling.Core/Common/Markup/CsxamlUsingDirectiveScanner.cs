using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Scans CSXAML source text for using directives.
/// </summary>
public static partial class CsxamlUsingDirectiveScanner
{
    /// <summary>
    /// Scans source text for using directives.
    /// </summary>
    /// <param name="text">The CSXAML source text to scan.</param>
    /// <returns>The using directives discovered in the source text.</returns>
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
