using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

public static partial class CsxamlNamespaceDirectiveScanner
{
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
