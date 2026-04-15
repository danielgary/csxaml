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
            var current = text[index];
            if (current is '"' or '\'')
            {
                index = SkipQuotedLiteral(text, index, current);
                continue;
            }

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
            var current = text[index];
            if (current is '"' or '\'')
            {
                index = SkipQuotedLiteral(text, index, current);
                continue;
            }

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
            var current = text[index];
            if (current is '"' or '\'')
            {
                index = SkipQuotedLiteral(text, index, current);
                continue;
            }

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
                return index;
            }
        }

        return text.Length - 1;
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
