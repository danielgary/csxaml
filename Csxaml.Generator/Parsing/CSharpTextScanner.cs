namespace Csxaml.Generator;

internal static class CSharpTextScanner
{
    public static bool TrySkipComment(SourceDocument source, int start, out int nextIndex)
    {
        var text = source.Text;
        nextIndex = start;

        if (start + 1 >= text.Length || text[start] != '/')
        {
            return false;
        }

        if (text[start + 1] == '/')
        {
            nextIndex = start + 2;

            while (nextIndex < text.Length &&
                   text[nextIndex] != '\r' &&
                   text[nextIndex] != '\n')
            {
                nextIndex++;
            }

            return true;
        }

        if (text[start + 1] != '*')
        {
            return false;
        }

        var commentEnd = text.IndexOf("*/", start + 2, StringComparison.Ordinal);
        if (commentEnd < 0)
        {
            throw DiagnosticFactory.FromPosition(source, start, "unterminated comment");
        }

        nextIndex = commentEnd + 2;
        return true;
    }

    public static bool TrySkipCommentOrLiteral(SourceDocument source, int start, out int nextIndex)
    {
        if (TrySkipComment(source, start, out nextIndex))
        {
            return true;
        }

        return TrySkipLiteral(source, start, out nextIndex);
    }

    public static int SkipWhitespaceAndComments(SourceDocument source, int start)
    {
        var text = source.Text;
        var index = start;

        while (index < text.Length)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index++;
                continue;
            }

            if (TrySkipComment(source, index, out var nextIndex))
            {
                index = nextIndex;
                continue;
            }

            break;
        }

        return index;
    }

    public static bool TryReadIdentifier(SourceDocument source, int start, out string identifier, out int end)
    {
        var text = source.Text;
        identifier = string.Empty;
        end = start;

        if (start >= text.Length || !IsIdentifierStart(text[start]))
        {
            return false;
        }

        end = start + 1;
        while (end < text.Length && IsIdentifierPart(text[end]))
        {
            end++;
        }

        identifier = text[start..end];
        return true;
    }

    public static bool IdentifierEqualsAt(SourceDocument source, int start, string identifier, out int end)
    {
        if (!TryReadIdentifier(source, start, out var actual, out end))
        {
            return false;
        }

        return string.Equals(actual, identifier, StringComparison.Ordinal);
    }

    public static TextSpan? TrimWhitespaceSpan(SourceDocument source, int start, int end)
    {
        var text = source.Text;
        var trimmedStart = start;
        var trimmedEnd = end;

        while (trimmedStart < trimmedEnd && char.IsWhiteSpace(text[trimmedStart]))
        {
            trimmedStart++;
        }

        while (trimmedEnd > trimmedStart && char.IsWhiteSpace(text[trimmedEnd - 1]))
        {
            trimmedEnd--;
        }

        if (trimmedStart >= trimmedEnd)
        {
            return null;
        }

        return new TextSpan(trimmedStart, trimmedEnd - trimmedStart);
    }

    private static bool TrySkipLiteral(SourceDocument source, int start, out int nextIndex)
    {
        var text = source.Text;
        nextIndex = start;

        if (start >= text.Length)
        {
            return false;
        }

        if (text[start] == '@' && start + 1 < text.Length && text[start + 1] == '"')
        {
            nextIndex = SkipVerbatimStringLiteral(source, start + 1);
            return true;
        }

        if (text[start] == '$')
        {
            return TrySkipInterpolatedLiteral(source, start, out nextIndex);
        }

        if (text[start] == '"')
        {
            nextIndex = CountQuoteRun(text, start) >= 3
                ? SkipRawStringLiteral(source, start)
                : SkipQuotedLiteral(source, start, '"');
            return true;
        }

        if (text[start] == '\'')
        {
            nextIndex = SkipQuotedLiteral(source, start, '\'');
            return true;
        }

        return false;
    }

    private static bool TrySkipInterpolatedLiteral(SourceDocument source, int start, out int nextIndex)
    {
        var text = source.Text;
        nextIndex = start;
        var index = start;

        while (index < text.Length && text[index] == '$')
        {
            index++;
        }

        if (index >= text.Length)
        {
            return false;
        }

        if (text[index] == '@' && index + 1 < text.Length && text[index + 1] == '"')
        {
            nextIndex = SkipVerbatimStringLiteral(source, index + 1);
            return true;
        }

        if (text[index] != '"')
        {
            return false;
        }

        nextIndex = CountQuoteRun(text, index) >= 3
            ? SkipRawStringLiteral(source, index)
            : SkipQuotedLiteral(source, index, '"');
        return true;
    }

    private static int SkipQuotedLiteral(SourceDocument source, int start, char delimiter)
    {
        var text = source.Text;
        var index = start + 1;

        while (index < text.Length)
        {
            if (text[index] == '\\')
            {
                index += 2;
                continue;
            }

            if (text[index] == delimiter)
            {
                return index + 1;
            }

            index++;
        }

        throw DiagnosticFactory.FromPosition(source, start, "unterminated string literal");
    }

    private static int SkipVerbatimStringLiteral(SourceDocument source, int quoteStart)
    {
        var text = source.Text;
        var index = quoteStart + 1;

        while (index < text.Length)
        {
            if (text[index] != '"')
            {
                index++;
                continue;
            }

            if (index + 1 < text.Length && text[index + 1] == '"')
            {
                index += 2;
                continue;
            }

            return index + 1;
        }

        throw DiagnosticFactory.FromPosition(source, quoteStart, "unterminated string literal");
    }

    private static int SkipRawStringLiteral(SourceDocument source, int quoteStart)
    {
        var text = source.Text;
        var quoteCount = CountQuoteRun(text, quoteStart);
        var index = quoteStart + quoteCount;

        while (index < text.Length)
        {
            if (text[index] != '"')
            {
                index++;
                continue;
            }

            if (CountQuoteRun(text, index) < quoteCount)
            {
                index++;
                continue;
            }

            return index + quoteCount;
        }

        throw DiagnosticFactory.FromPosition(source, quoteStart, "unterminated string literal");
    }

    private static int CountQuoteRun(string text, int start)
    {
        var index = start;
        while (index < text.Length && text[index] == '"')
        {
            index++;
        }

        return index - start;
    }

    private static bool IsIdentifierStart(char value)
    {
        return char.IsLetter(value) || value == '_';
    }

    private static bool IsIdentifierPart(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_' || value == '.';
    }
}
