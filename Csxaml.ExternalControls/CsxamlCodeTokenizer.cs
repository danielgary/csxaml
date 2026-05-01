namespace Csxaml.ExternalControls;

internal static class CsxamlCodeTokenizer
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "component",
        "foreach",
        "if",
        "inject",
        "namespace",
        "render",
        "resources",
        "startup",
        "using"
    };

    public static IReadOnlyList<CsxamlCodeToken> Tokenize(string code)
    {
        var tokens = new List<CsxamlCodeToken>();
        var index = 0;
        while (index < code.Length)
        {
            index = ReadNextToken(code, index, tokens);
        }

        return tokens;
    }

    private static int ReadNextToken(
        string code,
        int index,
        List<CsxamlCodeToken> tokens)
    {
        var current = code[index];
        if (current == '"')
        {
            return ReadUntil(code, index, tokens, '"', CsxamlCodeTokenKind.String);
        }

        if (current == '<')
        {
            return ReadUntil(code, index, tokens, '>', CsxamlCodeTokenKind.Markup);
        }

        if (current == '{')
        {
            return ReadUntil(code, index, tokens, '}', CsxamlCodeTokenKind.Expression);
        }

        if (char.IsLetter(current))
        {
            return ReadIdentifier(code, index, tokens);
        }

        tokens.Add(new CsxamlCodeToken(current.ToString(), CsxamlCodeTokenKind.Text));
        return index + 1;
    }

    private static int ReadIdentifier(
        string code,
        int index,
        List<CsxamlCodeToken> tokens)
    {
        var end = index + 1;
        while (end < code.Length && (char.IsLetterOrDigit(code[end]) || code[end] == '_'))
        {
            end++;
        }

        var text = code[index..end];
        var kind = Keywords.Contains(text)
            ? CsxamlCodeTokenKind.Keyword
            : CsxamlCodeTokenKind.Text;
        tokens.Add(new CsxamlCodeToken(text, kind));
        return end;
    }

    private static int ReadUntil(
        string code,
        int index,
        List<CsxamlCodeToken> tokens,
        char terminator,
        CsxamlCodeTokenKind kind)
    {
        var end = index + 1;
        while (end < code.Length)
        {
            if (code[end] == terminator)
            {
                end++;
                break;
            }

            end++;
        }

        tokens.Add(new CsxamlCodeToken(code[index..end], kind));
        return end;
    }
}
