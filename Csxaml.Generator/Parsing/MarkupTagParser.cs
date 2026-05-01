namespace Csxaml.Generator;

internal sealed class MarkupTagParser
{
    private readonly ParserContext _context;

    public MarkupTagParser(ParserContext context)
    {
        _context = context;
    }

    public bool IsClosingTag(MarkupTagName tagName)
    {
        if (!_context.PeekSymbol("<") || !_context.PeekSymbol("/", 1))
        {
            return false;
        }

        return TryPeekTagText(2, out var closingTagText) &&
            string.Equals(closingTagText, tagName.Text, StringComparison.Ordinal);
    }

    public MarkupTagName ParseTagName()
    {
        var firstPart = _context.ReadIdentifier("unsupported tag name");
        if (_context.TryReadSymbol(":"))
        {
            return ParseAliasQualifiedTag(firstPart);
        }

        if (_context.TryReadSymbol("."))
        {
            var localName = _context.ReadIdentifier("unsupported tag name");
            return new MarkupTagName(
                $"{firstPart.Text}.{localName.Text}",
                null,
                $"{firstPart.Text}.{localName.Text}",
                new TextSpan(firstPart.Span.Start, localName.Span.End - firstPart.Span.Start));
        }

        return new MarkupTagName(
            firstPart.Text,
            null,
            firstPart.Text,
            firstPart.Span);
    }

    public void ReadMatchingClosingTag(MarkupTagName tagName)
    {
        var firstPart = _context.ReadIdentifier($"unsupported tag name '{tagName.Text}'");
        var actualTagName = firstPart.Text;
        if (_context.TryReadSymbol(":"))
        {
            actualTagName = ReadAliasQualifiedClosingTag(tagName, firstPart);
        }
        else if (_context.TryReadSymbol("."))
        {
            actualTagName = $"{firstPart.Text}.{_context.ReadIdentifier($"unsupported tag name '{tagName.Text}'").Text}";
        }

        if (!string.Equals(actualTagName, tagName.Text, StringComparison.Ordinal))
        {
            throw _context.CreateException($"unsupported tag name '{tagName.Text}'");
        }
    }

    private MarkupTagName ParseAliasQualifiedTag(Token firstPart)
    {
        var localName = _context.ReadIdentifier("unsupported tag name");
        var text = $"{firstPart.Text}:{localName.Text}";
        if (_context.TryReadSymbol("."))
        {
            var propertyName = _context.ReadIdentifier("unsupported tag name");
            return new MarkupTagName(
                $"{text}.{propertyName.Text}",
                firstPart.Text,
                $"{localName.Text}.{propertyName.Text}",
                new TextSpan(firstPart.Span.Start, propertyName.Span.End - firstPart.Span.Start));
        }

        return new MarkupTagName(
            text,
            firstPart.Text,
            localName.Text,
            new TextSpan(firstPart.Span.Start, localName.Span.End - firstPart.Span.Start));
    }

    private string ReadAliasQualifiedClosingTag(MarkupTagName tagName, Token firstPart)
    {
        var localName = _context.ReadIdentifier($"unsupported tag name '{tagName.Text}'");
        var actualTagName = $"{firstPart.Text}:{localName.Text}";
        if (_context.TryReadSymbol("."))
        {
            actualTagName = $"{actualTagName}.{_context.ReadIdentifier($"unsupported tag name '{tagName.Text}'").Text}";
        }

        return actualTagName;
    }

    private bool TryPeekTagText(int offset, out string tagText)
    {
        tagText = string.Empty;
        if (_context.Peek(offset).Kind != TokenKind.Identifier)
        {
            return false;
        }

        var firstPart = _context.Peek(offset).Text;
        if (_context.PeekSymbol(":", offset + 1) && _context.Peek(offset + 2).Kind == TokenKind.Identifier)
        {
            tagText = $"{firstPart}:{_context.Peek(offset + 2).Text}";
            if (_context.PeekSymbol(".", offset + 3) && _context.Peek(offset + 4).Kind == TokenKind.Identifier)
            {
                tagText = $"{tagText}.{_context.Peek(offset + 4).Text}";
            }

            return true;
        }

        if (_context.PeekSymbol(".", offset + 1) && _context.Peek(offset + 2).Kind == TokenKind.Identifier)
        {
            tagText = $"{firstPart}.{_context.Peek(offset + 2).Text}";
            return true;
        }

        tagText = firstPart;
        return true;
    }
}
