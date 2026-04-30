namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Analyzes source text to identify the CSXAML markup context at a cursor position.
/// </summary>
public static partial class CsxamlMarkupContextAnalyzer
{
    /// <summary>
    /// Analyzes the markup context at the specified source position.
    /// </summary>
    /// <param name="text">The CSXAML source text to analyze.</param>
    /// <param name="position">The zero-based cursor offset.</param>
    /// <returns>The markup context at the position.</returns>
    public static CsxamlMarkupContext Analyze(string text, int position)
    {
        if (position < 0 || position > text.Length)
        {
            return None();
        }

        var tagStart = FindOpenTagStart(text, position);
        if (tagStart < 0)
        {
            return None();
        }

        var tagState = ParseOpenTagState(text, tagStart, position);
        return tagState.Kind switch
        {
            CsxamlMarkupContextKind.TagName => new CsxamlMarkupContext(
                CsxamlMarkupContextKind.TagName,
                tagState.PrefixText,
                tagState.Qualifier,
                null,
                Array.Empty<string>(),
                tagState.PropertyContentOwner),
            CsxamlMarkupContextKind.AttributeName => new CsxamlMarkupContext(
                CsxamlMarkupContextKind.AttributeName,
                tagState.PrefixText,
                null,
                tagState.TagName,
                tagState.ExistingAttributes,
                null),
            _ => None(),
        };
    }

    private static int FindOpenTagStart(string text, int position)
    {
        var braceDepth = 0;
        var activeStart = -1;
        var inString = false;
        var stringDelimiter = '\0';

        for (var index = 0; index < position; index++)
        {
            var current = text[index];
            if (inString)
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == stringDelimiter)
                {
                    inString = false;
                }

                continue;
            }

            if (current is '"' or '\'')
            {
                inString = true;
                stringDelimiter = current;
                continue;
            }

            if (activeStart >= 0 && current == '{')
            {
                braceDepth++;
                continue;
            }

            if (activeStart >= 0 && current == '}' && braceDepth > 0)
            {
                braceDepth--;
                continue;
            }

            if (current == '<' && braceDepth == 0)
            {
                activeStart = index;
                continue;
            }

            if (braceDepth == 0 && current == '>')
            {
                activeStart = -1;
            }
        }

        return activeStart;
    }

    private static TagState ParseOpenTagState(string text, int tagStart, int position)
    {
        var index = tagStart + 1;
        if (index < text.Length && text[index] == '/')
        {
            index++;
        }

        index = CsxamlTextScanner.SkipWhitespace(text, index);
        var nameStart = index;
        while (index < position && index < text.Length && IsTagNameCharacter(text[index]))
        {
            index++;
        }

        var rawTagText = text[nameStart..index];
        if (position <= index)
        {
            if (TrySplitPropertyContent(rawTagText, out var ownerName, out var propertyPrefix))
            {
                return new TagState(
                    CsxamlMarkupContextKind.TagName,
                    propertyPrefix,
                    null,
                    null,
                    Array.Empty<string>(),
                    ownerName);
            }

            var qualifierSeparator = rawTagText.IndexOf(':');
            return new TagState(
                CsxamlMarkupContextKind.TagName,
                qualifierSeparator >= 0 ? rawTagText[(qualifierSeparator + 1)..] : rawTagText,
                qualifierSeparator >= 0 ? rawTagText[..qualifierSeparator] : null,
                null,
                Array.Empty<string>(),
                null);
        }

        if (string.IsNullOrWhiteSpace(rawTagText))
        {
            return new TagState(CsxamlMarkupContextKind.TagName, string.Empty, null, null, Array.Empty<string>(), null);
        }

        var tagName = rawTagText;
        var attributes = ReadExistingAttributeNames(text, index, position);
        var attributePrefix = ReadAttributePrefix(text, index, position);
        return attributePrefix is null
            ? new TagState(CsxamlMarkupContextKind.None, string.Empty, null, null, Array.Empty<string>(), null)
            : new TagState(
                CsxamlMarkupContextKind.AttributeName,
                attributePrefix,
                null,
                tagName,
                attributes,
                null);
    }

    private static IReadOnlyList<string> ReadExistingAttributeNames(string text, int start, int end)
    {
        var names = new List<string>();
        var index = start;
        while (index < end)
        {
            index = CsxamlTextScanner.SkipWhitespace(text, index);
            if (index >= end || text[index] is '>' or '/')
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
            names.Add(text[nameStart..index]);
            index = SkipAttributeValue(text, index, end);
        }

        return names;
    }

    private static string? ReadAttributePrefix(string text, int start, int end)
    {
        var index = start;
        var lastIdentifierStart = -1;
        var lastIdentifierEnd = -1;

        while (index < end)
        {
            index = CsxamlTextScanner.SkipWhitespace(text, index);
            if (index >= end || text[index] is '>' or '/')
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
            var nameEnd = index;
            if (nameEnd == end)
            {
                return text[nameStart..nameEnd];
            }

            lastIdentifierStart = nameStart;
            lastIdentifierEnd = nameEnd;
            index = SkipAttributeValue(text, index, end);
        }

        if (lastIdentifierStart >= 0 && end <= lastIdentifierEnd)
        {
            return text[lastIdentifierStart..end];
        }

        return string.Empty;
    }

    private static int SkipAttributeValue(string text, int index, int end)
    {
        index = CsxamlTextScanner.SkipWhitespace(text, index);
        if (index >= end || text[index] != '=')
        {
            return index;
        }

        index = CsxamlTextScanner.SkipWhitespace(text, index + 1);
        if (index >= end)
        {
            return index;
        }

        if (text[index] == '"')
        {
            var closingQuote = text.IndexOf('"', index + 1);
            return closingQuote < 0 || closingQuote >= end ? end : closingQuote + 1;
        }

        if (text[index] == '{')
        {
            var closingBrace = CsxamlTextScanner.FindMatchingDelimiter(text, index, '{', '}');
            return closingBrace < 0 || closingBrace >= end ? end : closingBrace + 1;
        }

        return index;
    }

}
