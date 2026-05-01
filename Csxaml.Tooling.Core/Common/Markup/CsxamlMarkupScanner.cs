using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Scans CSXAML source text for top-level directives and markup references.
/// </summary>
public static partial class CsxamlMarkupScanner
{
    /// <summary>
    /// Scans source text for directives, component signatures, and markup element references.
    /// </summary>
    /// <param name="text">The CSXAML source text to scan.</param>
    /// <returns>The scan result for the source text.</returns>
    public static CsxamlMarkupScanResult Scan(string text)
    {
        var elements = new List<CsxamlMarkupElementReference>();
        foreach (Match match in TagPattern().Matches(text))
        {
            var tagName = match.Groups["tag"].Value;
            var tagNameStart = match.Groups["tag"].Index;
            var tagEnd = CsxamlTextScanner.FindTagEnd(text, tagNameStart + tagName.Length);
            if (tagEnd < 0)
            {
                continue;
            }

            elements.Add(
                CreateElementReference(
                    text,
                    tagName,
                    tagNameStart,
                    match.Groups["slash"].Value == "/",
                    match.Groups["open"].Index,
                    tagEnd));
        }

        return new CsxamlMarkupScanResult(
            CsxamlUsingDirectiveScanner.Scan(text),
            CsxamlNamespaceDirectiveScanner.Scan(text),
            CsxamlComponentSignatureScanner.Scan(text),
            elements);
    }

    private static CsxamlMarkupElementReference CreateElementReference(
        string text,
        string tagName,
        int tagNameStart,
        bool isClosing,
        int openTagStart,
        int tagEnd)
    {
        var separatorIndex = tagName.IndexOf(':');
        var prefix = separatorIndex >= 0 ? tagName[..separatorIndex] : null;
        var localName = separatorIndex >= 0 ? tagName[(separatorIndex + 1)..] : tagName;
        var propertySeparatorIndex = tagName.LastIndexOf('.');
        var propertyOwner = propertySeparatorIndex > 0 ? tagName[..propertySeparatorIndex] : null;
        var propertyName = propertySeparatorIndex > 0 && propertySeparatorIndex < tagName.Length - 1
            ? tagName[(propertySeparatorIndex + 1)..]
            : null;
        var propertyNameStart = propertyName is null
            ? -1
            : tagNameStart + propertySeparatorIndex + 1;

        return new CsxamlMarkupElementReference(
            tagName,
            prefix,
            localName,
            tagNameStart,
            tagName.Length,
            openTagStart,
            tagEnd,
            isClosing,
            isClosing
                ? Array.Empty<CsxamlMarkupAttributeReference>()
                : ReadAttributes(text, tagNameStart + tagName.Length, tagEnd),
            propertyOwner,
            propertyName,
            propertyOwner is null ? -1 : tagNameStart,
            propertyOwner?.Length ?? 0,
            propertyNameStart,
            propertyName?.Length ?? 0);
    }

    private static IReadOnlyList<CsxamlMarkupAttributeReference> ReadAttributes(
        string text,
        int start,
        int end)
    {
        var attributes = new List<CsxamlMarkupAttributeReference>();
        var index = start;
        while (index < end)
        {
            index = CsxamlTextScanner.SkipWhitespace(text, index);
            if (index >= end || text[index] is '/' or '>')
            {
                break;
            }

            if (!CsxamlTextScanner.IsIdentifierStart(text[index]))
            {
                index++;
                continue;
            }

            var nameStart = index;
            index = CsxamlTextScanner.ReadIdentifier(text, index);
            var attributeName = text[nameStart..index];
            attributes.Add(new CsxamlMarkupAttributeReference(attributeName, nameStart, attributeName.Length));

            index = CsxamlTextScanner.SkipWhitespace(text, index);
            if (index >= end || text[index] != '=')
            {
                continue;
            }

            index = CsxamlTextScanner.SkipWhitespace(text, index + 1);
            if (index >= end)
            {
                continue;
            }

            if (text[index] == '"')
            {
                index = text.IndexOf('"', index + 1);
                index = index < 0 ? end : index + 1;
                continue;
            }

            if (text[index] == '{')
            {
                var expressionEnd = CsxamlTextScanner.FindMatchingDelimiter(text, index, '{', '}');
                index = expressionEnd < 0 ? end : expressionEnd + 1;
            }
        }

        return attributes;
    }

    [GeneratedRegex(
        @"(^|[\s{;(])<(?<slash>/?)(?<tag>[A-Za-z_][A-Za-z0-9_:.]*)(?=[\s/>])",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();
}
