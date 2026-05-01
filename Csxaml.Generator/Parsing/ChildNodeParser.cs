namespace Csxaml.Generator;

internal sealed class ChildNodeParser
{
    private readonly ParserContext _context;
    private readonly MarkupNodeParser _markupNodeParser;

    public ChildNodeParser(ParserContext context)
    {
        _context = context;
        _markupNodeParser = new MarkupNodeParser(context, ParseChildNode);
    }

    public ChildNode ParseRootNode()
    {
        return ParseChildNode(
            "render statement must contain a single markup root",
            allowPropertyContent: false);
    }

    private IReadOnlyList<ChildNode> ParseBlockChildren(string message, bool allowPropertyContent)
    {
        var children = new List<ChildNode>();
        while (!_context.TryReadSymbol("}"))
        {
            children.Add(ParseChildNode(message, allowPropertyContent));
        }

        return children;
    }

    private ChildNode ParseChildNode(string message, bool allowPropertyContent)
    {
        if (_context.TryReadIdentifier("if"))
        {
            return ParseIfBlock(allowPropertyContent);
        }

        if (_context.TryReadIdentifier("foreach"))
        {
            return ParseForEachBlock(allowPropertyContent);
        }

        return _markupNodeParser.ParseMarkupNode(message, allowPropertyContent);
    }

    private ForEachBlockNode ParseForEachBlock(bool allowPropertyContent)
    {
        const string message = "invalid foreach block";
        var start = _context.PreviousTokenStart;
        _context.ReadSymbol("(", message);
        _context.ReadIdentifier("var", message);
        var itemName = _context.ReadIdentifier(message);
        _context.ReadIdentifier("in", message);
        var collectionStart = _context.Current.Span.Start;
        var collectionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message, allowPropertyContent);
        return new ForEachBlockNode(
            itemName.Text,
            collectionExpression,
            new TextSpan(collectionStart, Math.Max(collectionExpression.Length, 0)),
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }

    private IfBlockNode ParseIfBlock(bool allowPropertyContent)
    {
        const string message = "invalid if block";
        var start = _context.PreviousTokenStart;
        _context.ReadSymbol("(", message);
        var conditionStart = _context.Current.Span.Start;
        var conditionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message, allowPropertyContent);
        return new IfBlockNode(
            conditionExpression,
            new TextSpan(conditionStart, Math.Max(conditionExpression.Length, 0)),
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }
}
