namespace Csxaml.Generator;

internal sealed class Tokenizer
{
    public IReadOnlyList<Token> Tokenize(SourceDocument source)
    {
        var tokens = new List<Token>();
        var index = 0;

        while (index < source.Text.Length)
        {
            var current = source.Text[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                index = ReadIdentifier(source.Text, index, tokens);
                continue;
            }

            if (current == '"')
            {
                index = ReadStringLiteral(source, index, tokens);
                continue;
            }

            if (current == '=' && index + 1 < source.Text.Length && source.Text[index + 1] == '>')
            {
                tokens.Add(new Token(TokenKind.Symbol, "=>", new TextSpan(index, 2)));
                index += 2;
                continue;
            }

            tokens.Add(new Token(TokenKind.Symbol, current.ToString(), new TextSpan(index, 1)));
            index++;
        }

        tokens.Add(new Token(TokenKind.EndOfFile, string.Empty, new TextSpan(source.Text.Length, 0)));
        return tokens;
    }

    private static int ReadIdentifier(string source, int start, ICollection<Token> tokens)
    {
        var index = start + 1;
        while (index < source.Length && (char.IsLetterOrDigit(source[index]) || source[index] == '_'))
        {
            index++;
        }

        tokens.Add(new Token(
            TokenKind.Identifier,
            source[start..index],
            new TextSpan(start, index - start)));

        return index;
    }

    private static int ReadStringLiteral(
        SourceDocument source,
        int start,
        ICollection<Token> tokens)
    {
        var index = start + 1;
        while (index < source.Text.Length)
        {
            if (source.Text[index] == '\\')
            {
                index += 2;
                continue;
            }

            if (source.Text[index] == '"')
            {
                tokens.Add(new Token(
                    TokenKind.StringLiteral,
                    source.Text[(start + 1)..index],
                    new TextSpan(start, index - start + 1)));

                return index + 1;
            }

            index++;
        }

        throw DiagnosticFactory.FromPosition(source, start, "unterminated string literal");
    }
}
