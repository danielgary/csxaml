using System.Text;

namespace Csxaml.Tooling.Core.Markup;

internal static class CsxamlTextScanner
{
    public static int FindMatchingDelimiter(
        string text,
        int startIndex,
        char openingDelimiter,
        char closingDelimiter)
    {
        var delimiters = new Stack<char>();
        delimiters.Push(closingDelimiter);

        for (var index = startIndex + 1; index < text.Length; index++)
        {
            if (TrySkipCommentOrLiteral(text, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = text[index];
            if (TryGetClosingDelimiter(current, out var nestedClosing))
            {
                delimiters.Push(nestedClosing);
                continue;
            }

            if (current != delimiters.Peek())
            {
                continue;
            }

            delimiters.Pop();
            if (delimiters.Count == 0)
            {
                return index;
            }
        }

        return -1;
    }

    public static int FindTagEnd(string text, int startIndex)
    {
        var braceDepth = 0;
        for (var index = startIndex; index < text.Length; index++)
        {
            if (TrySkipCommentOrLiteral(text, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = text[index];
            if (current == '{')
            {
                braceDepth++;
                continue;
            }

            if (current == '}' && braceDepth > 0)
            {
                braceDepth--;
                continue;
            }

            if (current == '>' && braceDepth == 0)
            {
                return index;
            }
        }

        return -1;
    }

    public static bool IsIdentifierStart(char character)
    {
        return char.IsLetter(character) || character == '_';
    }

    public static int ReadIdentifier(string text, int startIndex)
    {
        var index = startIndex;
        while (index < text.Length && IsIdentifierPart(text[index]))
        {
            index++;
        }

        return index;
    }

    public static int SkipWhitespace(string text, int startIndex)
    {
        var index = startIndex;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    public static IReadOnlyList<TopLevelTextSegment> SplitTopLevelRanges(
        string text,
        int offset,
        char separator)
    {
        var segments = new List<TopLevelTextSegment>();
        var start = 0;
        var delimiters = new Stack<char>();

        for (var index = 0; index < text.Length; index++)
        {
            if (TrySkipCommentOrLiteral(text, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = text[index];
            if (TryGetClosingDelimiter(current, out var closingDelimiter))
            {
                delimiters.Push(closingDelimiter);
                continue;
            }

            if (delimiters.Count > 0 && current == delimiters.Peek())
            {
                delimiters.Pop();
                continue;
            }

            if (delimiters.Count != 0 || current != separator)
            {
                continue;
            }

            segments.Add(
                new TopLevelTextSegment(
                    text[start..index],
                    offset + start,
                    offset + index));
            start = index + 1;
        }

        segments.Add(
            new TopLevelTextSegment(
                text[start..],
                offset + start,
                offset + text.Length));
        return segments;
    }

    public static string ReadCurrentIdentifierFragment(string text, int start, int endExclusive)
    {
        var builder = new StringBuilder();
        for (var index = start; index < endExclusive && index < text.Length; index++)
        {
            if (!IsIdentifierPart(text[index]))
            {
                break;
            }

            builder.Append(text[index]);
        }

        return builder.ToString();
    }

    private static bool IsIdentifierPart(char character)
    {
        return char.IsLetterOrDigit(character) || character is '_' or ':' or '.';
    }

    private static bool TrySkipCommentOrLiteral(string text, int startIndex, out int nextIndex)
    {
        if (TrySkipComment(text, startIndex, out nextIndex))
        {
            return true;
        }

        return TrySkipLiteral(text, startIndex, out nextIndex);
    }

    private static bool TrySkipComment(string text, int startIndex, out int nextIndex)
    {
        nextIndex = startIndex;
        if (startIndex + 1 >= text.Length || text[startIndex] != '/')
        {
            return false;
        }

        if (text[startIndex + 1] == '/')
        {
            nextIndex = startIndex + 2;
            while (nextIndex < text.Length && text[nextIndex] is not '\r' and not '\n')
            {
                nextIndex++;
            }

            return true;
        }

        if (text[startIndex + 1] != '*')
        {
            return false;
        }

        var commentEnd = text.IndexOf("*/", startIndex + 2, StringComparison.Ordinal);
        nextIndex = commentEnd < 0 ? text.Length : commentEnd + 2;
        return true;
    }

    private static bool TrySkipLiteral(string text, int startIndex, out int nextIndex)
    {
        nextIndex = startIndex;
        if (startIndex >= text.Length)
        {
            return false;
        }

        if (text[startIndex] == '@' && startIndex + 1 < text.Length && text[startIndex + 1] == '"')
        {
            nextIndex = SkipVerbatimStringLiteral(text, startIndex + 1);
            return true;
        }

        if (text[startIndex] == '$')
        {
            return TrySkipInterpolatedLiteral(text, startIndex, out nextIndex);
        }

        if (text[startIndex] == '"')
        {
            nextIndex = CountQuoteRun(text, startIndex) >= 3
                ? SkipRawStringLiteral(text, startIndex)
                : SkipQuotedLiteral(text, startIndex, '"');
            return true;
        }

        if (text[startIndex] == '\'')
        {
            nextIndex = SkipQuotedLiteral(text, startIndex, '\'');
            return true;
        }

        return false;
    }

    private static int SkipQuotedLiteral(string text, int startIndex, char quoteCharacter)
    {
        for (var index = startIndex + 1; index < text.Length; index++)
        {
            if (text[index] == '\\')
            {
                index++;
                continue;
            }

            if (text[index] == quoteCharacter)
            {
                return index + 1;
            }
        }

        return text.Length;
    }

    private static bool TrySkipInterpolatedLiteral(string text, int startIndex, out int nextIndex)
    {
        nextIndex = startIndex;
        var index = startIndex;
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
            nextIndex = SkipVerbatimStringLiteral(text, index + 1);
            return true;
        }

        if (text[index] != '"')
        {
            return false;
        }

        nextIndex = CountQuoteRun(text, index) >= 3
            ? SkipRawStringLiteral(text, index)
            : SkipQuotedLiteral(text, index, '"');
        return true;
    }

    private static int SkipVerbatimStringLiteral(string text, int quoteStart)
    {
        for (var index = quoteStart + 1; index < text.Length; index++)
        {
            if (text[index] != '"')
            {
                continue;
            }

            if (index + 1 < text.Length && text[index + 1] == '"')
            {
                index++;
                continue;
            }

            return index + 1;
        }

        return text.Length;
    }

    private static int SkipRawStringLiteral(string text, int quoteStart)
    {
        var quoteCount = CountQuoteRun(text, quoteStart);
        for (var index = quoteStart + quoteCount; index < text.Length; index++)
        {
            if (text[index] != '"')
            {
                continue;
            }

            if (CountQuoteRun(text, index) < quoteCount)
            {
                continue;
            }

            return index + quoteCount;
        }

        return text.Length;
    }

    private static int CountQuoteRun(string text, int startIndex)
    {
        var index = startIndex;
        while (index < text.Length && text[index] == '"')
        {
            index++;
        }

        return index - startIndex;
    }

    private static bool TryGetClosingDelimiter(char openingDelimiter, out char closingDelimiter)
    {
        closingDelimiter = openingDelimiter switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            '<' => '>',
            _ => '\0',
        };
        return closingDelimiter != '\0';
    }
}
