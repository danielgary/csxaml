using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

public static partial class CsxamlMarkupScanner
{
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
                : ReadAttributes(text, tagNameStart + tagName.Length, tagEnd));
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
        @"(^|[\s{;(])<(?<slash>/?)(?<tag>[A-Za-z_][A-Za-z0-9_:]*)(?=[\s/>])",
        RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();
}
