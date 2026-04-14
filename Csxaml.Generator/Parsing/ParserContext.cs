namespace Csxaml.Generator;

internal sealed class ParserContext
{
    private readonly SourceDocument _source;
    private readonly IReadOnlyList<Token> _tokens;
    private Token? _previousToken;
    private int _position;

    public ParserContext(SourceDocument source, IReadOnlyList<Token> tokens)
    {
        _source = source;
        _tokens = tokens;
    }

    public Token Current => Peek();

    public int PreviousTokenStart => _previousToken?.Span.Start ?? 0;

    public SourceDocument Source => _source;

    public bool PeekIdentifier(string value, int offset = 0)
    {
        var token = Peek(offset);
        return token.Kind == TokenKind.Identifier &&
               string.Equals(token.Text, value, StringComparison.Ordinal);
    }

    public bool PeekSymbol(string value, int offset = 0)
    {
        var token = Peek(offset);
        return token.Kind == TokenKind.Symbol &&
               string.Equals(token.Text, value, StringComparison.Ordinal);
    }

    public Token Peek(int offset = 0)
    {
        var index = Math.Min(_position + offset, _tokens.Count - 1);
        return _tokens[index];
    }

    public Token ReadCurrent(string message)
    {
        if (Current.Kind == TokenKind.EndOfFile)
        {
            throw CreateException(message);
        }

        return Consume();
    }

    public void ReadEndOfFile(string message)
    {
        if (Current.Kind != TokenKind.EndOfFile)
        {
            throw CreateException(message);
        }
    }

    public Token ReadIdentifier(string message)
    {
        if (Current.Kind != TokenKind.Identifier)
        {
            throw CreateException(message);
        }

        return Consume();
    }

    public Token ReadIdentifier(string value, string message)
    {
        if (!PeekIdentifier(value))
        {
            throw CreateException(message);
        }

        return Consume();
    }

    public string ReadRawUntilMatching(char closingDelimiter, string message)
    {
        var start = (_previousToken?.Span.End).GetValueOrDefault();
        var end = ScanToMatchingDelimiter(start, closingDelimiter, message);
        AdvanceToPosition(end);
        return _source.Text[start..end];
    }

    public void SkipToPosition(int position)
    {
        AdvanceToPosition(position);
    }

    public Token ReadStringLiteral(string message)
    {
        if (Current.Kind != TokenKind.StringLiteral)
        {
            throw CreateException(message);
        }

        return Consume();
    }

    public Token ReadSymbol(string value, string message)
    {
        if (!PeekSymbol(value))
        {
            throw CreateException(message);
        }

        return Consume();
    }

    public bool TryReadIdentifier(string value)
    {
        if (!PeekIdentifier(value))
        {
            return false;
        }

        Consume();
        return true;
    }

    public bool TryReadSymbol(string value)
    {
        if (!PeekSymbol(value))
        {
            return false;
        }

        Consume();
        return true;
    }

    public DiagnosticException CreateException(string message)
    {
        return DiagnosticFactory.FromSpan(_source, Current.Span, message);
    }

    public DiagnosticException CreateException(TextSpan span, string message)
    {
        return DiagnosticFactory.FromSpan(_source, span, message);
    }

    private void AdvanceToPosition(int position)
    {
        while (Current.Kind != TokenKind.EndOfFile && Current.Span.Start < position)
        {
            _position++;
        }
    }

    private Token Consume()
    {
        _previousToken = Current;
        _position++;
        return _previousToken.Value;
    }

    private int ScanToMatchingDelimiter(int start, char closingDelimiter, string message)
    {
        var delimiters = new Stack<char>();
        for (var index = start; index < _source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(_source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = _source.Text[index];
            if (current == '"' || current == '\'')
            {
                continue;
            }

            if (current is '(' or '[' or '{')
            {
                delimiters.Push(GetClosingDelimiter(current));
                continue;
            }

            if (delimiters.Count > 0 && current == delimiters.Peek())
            {
                delimiters.Pop();
                continue;
            }

            if (delimiters.Count == 0 && current == closingDelimiter)
            {
                return index;
            }
        }

        throw CreateException(message);
    }

    private static char GetClosingDelimiter(char openingDelimiter)
    {
        return openingDelimiter switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            _ => throw new InvalidOperationException(
                $"Unsupported opening delimiter '{openingDelimiter}'.")
        };
    }
}
