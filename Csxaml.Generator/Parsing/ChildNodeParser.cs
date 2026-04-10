namespace Csxaml.Generator;

internal sealed class ChildNodeParser
{
    private readonly ParserContext _context;

    public ChildNodeParser(ParserContext context)
    {
        _context = context;
    }

    public MarkupNode ParseRootNode()
    {
        return ParseMarkupNode("missing return block");
    }

    private IReadOnlyList<ChildNode> ParseBlockChildren(string message)
    {
        var children = new List<ChildNode>();
        while (!_context.TryReadSymbol("}"))
        {
            children.Add(ParseChildNode(message));
        }

        return children;
    }

    private ChildNode ParseChildNode(string message)
    {
        if (_context.TryReadIdentifier("if"))
        {
            return ParseIfBlock();
        }

        if (_context.TryReadIdentifier("foreach"))
        {
            return ParseForEachBlock();
        }

        return ParseMarkupNode(message);
    }

    private ForEachBlockNode ParseForEachBlock()
    {
        const string message = "invalid foreach block";
        var start = _context.PreviousTokenStart;
        _context.ReadSymbol("(", message);
        _context.ReadIdentifier("var", message);
        var itemName = _context.ReadIdentifier(message);
        _context.ReadIdentifier("in", message);
        var collectionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message);
        return new ForEachBlockNode(
            itemName.Text,
            collectionExpression,
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }

    private IfBlockNode ParseIfBlock()
    {
        const string message = "invalid if block";
        var start = _context.PreviousTokenStart;
        _context.ReadSymbol("(", message);
        var conditionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message);
        return new IfBlockNode(
            conditionExpression,
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }

    private MarkupNode ParseMarkupNode(string message)
    {
        var openAngle = _context.ReadSymbol("<", message);
        if (_context.TryReadSymbol("/"))
        {
            throw _context.CreateException(openAngle.Span, "unsupported tag name");
        }

        var tagName = _context.ReadIdentifier("unsupported tag name");
        var properties = ParseProperties(tagName);
        return _context.TryReadSymbol("/")
            ? ParseSelfClosingNode(openAngle, tagName, properties)
            : ParseElementNode(openAngle, tagName, properties);
    }

    private PropertyNode ParseProperty()
    {
        var propertyName = _context.ReadIdentifier("unsupported prop name");
        _context.ReadSymbol("=", $"unsupported prop name '{propertyName.Text}'");

        if (_context.TryReadSymbol("{"))
        {
            var value = _context.ReadRawUntilMatching('}', $"unsupported prop name '{propertyName.Text}'").Trim();
            var closeBrace = _context.ReadSymbol("}", $"unsupported prop name '{propertyName.Text}'");
            return new PropertyNode(
                propertyName.Text,
                PropertyValueKind.Expression,
                value,
                new TextSpan(propertyName.Span.Start, closeBrace.Span.End - propertyName.Span.Start));
        }

        var stringToken = _context.ReadStringLiteral($"unsupported prop name '{propertyName.Text}'");
        return new PropertyNode(
            propertyName.Text,
            PropertyValueKind.StringLiteral,
            stringToken.Text,
            new TextSpan(propertyName.Span.Start, stringToken.Span.End - propertyName.Span.Start));
    }

    private IReadOnlyList<PropertyNode> ParseProperties(Token tagName)
    {
        var properties = new List<PropertyNode>();
        while (!_context.PeekSymbol(">") &&
               !(_context.PeekSymbol("/") && _context.PeekSymbol(">", 1)))
        {
            properties.Add(ParseProperty());
        }

        return properties;
    }

    private MarkupNode ParseSelfClosingNode(
        Token openAngle,
        Token tagName,
        IReadOnlyList<PropertyNode> properties)
    {
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName.Text,
            properties,
            Array.Empty<ChildNode>(),
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private MarkupNode ParseElementNode(
        Token openAngle,
        Token tagName,
        IReadOnlyList<PropertyNode> properties)
    {
        _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        var children = new List<ChildNode>();
        while (!IsClosingTag(tagName.Text))
        {
            children.Add(ParseChildNode($"unsupported tag name '{tagName.Text}'"));
        }

        _context.ReadSymbol("<", $"unsupported tag name '{tagName.Text}'");
        _context.ReadSymbol("/", $"unsupported tag name '{tagName.Text}'");
        _context.ReadIdentifier(tagName.Text, $"unsupported tag name '{tagName.Text}'");
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName.Text,
            properties,
            children,
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private bool IsClosingTag(string tagName)
    {
        return _context.PeekSymbol("<") &&
               _context.PeekSymbol("/", 1) &&
               _context.PeekIdentifier(tagName, 2);
    }
}
