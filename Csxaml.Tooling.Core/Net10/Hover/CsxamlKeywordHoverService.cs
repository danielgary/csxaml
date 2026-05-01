namespace Csxaml.Tooling.Core.Hover;

internal static class CsxamlKeywordHoverService
{
    public static CsxamlHoverInfo? TryGetHover(string text, int position)
    {
        if (position < 0 || position > text.Length)
        {
            return null;
        }

        var start = FindWordStart(text, position);
        var end = FindWordEnd(text, position);
        if (start < 0 ||
            end <= start ||
            !string.Equals(text[start..end], "foreach", StringComparison.Ordinal) ||
            !IsInsideRenderMarkup(text, start))
        {
            return null;
        }

        return new CsxamlHoverInfo(start, end - start, CsxamlKeywordHoverFormatter.FormatForeachKeyword());
    }

    private static bool IsInsideRenderMarkup(string text, int position)
    {
        var renderIndex = FindLastStandaloneWord(text, "render", position);
        if (renderIndex < 0)
        {
            return false;
        }

        var markupStart = text.IndexOf('<', renderIndex + "render".Length);
        if (markupStart < 0 || markupStart > position)
        {
            return false;
        }

        var renderEnd = text.IndexOf(';', markupStart);
        return renderEnd < 0 || renderEnd > position;
    }

    private static int FindLastStandaloneWord(string text, string word, int beforePosition)
    {
        var startIndex = Math.Min(beforePosition, text.Length - 1);
        while (startIndex >= 0)
        {
            var candidate = text.LastIndexOf(word, startIndex, StringComparison.Ordinal);
            if (candidate < 0)
            {
                return -1;
            }

            if (IsWordBoundary(text, candidate - 1) &&
                IsWordBoundary(text, candidate + word.Length))
            {
                return candidate;
            }

            startIndex = candidate - 1;
        }

        return -1;
    }

    private static int FindWordStart(string text, int position)
    {
        var index = Math.Min(position, text.Length - 1);
        if (index < 0)
        {
            return -1;
        }

        while (index > 0 && IsWordCharacter(text[index - 1]))
        {
            index--;
        }

        return IsWordCharacter(text[index]) ? index : -1;
    }

    private static int FindWordEnd(string text, int position)
    {
        var index = Math.Min(position, text.Length);
        while (index < text.Length && IsWordCharacter(text[index]))
        {
            index++;
        }

        return index;
    }

    private static bool IsWordCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character == '_';
    }

    private static bool IsWordBoundary(string text, int index)
    {
        return index < 0 || index >= text.Length || !IsWordCharacter(text[index]);
    }
}
