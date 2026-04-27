using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Scans CSXAML source text for a namespace directive.
/// </summary>
public static partial class CsxamlNamespaceDirectiveScanner
{
    /// <summary>
    /// Scans source text for the namespace directive.
    /// </summary>
    /// <param name="text">The CSXAML source text to scan.</param>
    /// <returns>The namespace directive, or <see langword="null"/> when none is present.</returns>
    public static CsxamlNamespaceDirectiveInfo? Scan(string text)
    {
        var match = NamespaceDirectivePattern().Match(text);
        if (!match.Success)
        {
            return null;
        }

        var namespaceGroup = match.Groups["namespace"];
        return new CsxamlNamespaceDirectiveInfo(
            namespaceGroup.Value,
            match.Index,
            match.Length);
    }

    [GeneratedRegex(
        @"^\s*namespace\s+(?<namespace>[A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex NamespaceDirectivePattern();
}
