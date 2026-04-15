namespace Csxaml.Generator;

internal sealed class ChildNodeParser
{
    private readonly ParserContext _context;

    public ChildNodeParser(ParserContext context)
    {
        _context = context;
    }

    public ChildNode ParseRootNode()
    {
        return ParseChildNode("render statement must contain a single markup root");
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
        var collectionStart = _context.Current.Span.Start;
        var collectionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message);
        return new ForEachBlockNode(
            itemName.Text,
            collectionExpression,
            new TextSpan(collectionStart, Math.Max(collectionExpression.Length, 0)),
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }

    private IfBlockNode ParseIfBlock()
    {
        const string message = "invalid if block";
        var start = _context.PreviousTokenStart;
        _context.ReadSymbol("(", message);
        var conditionStart = _context.Current.Span.Start;
        var conditionExpression = _context.ReadRawUntilMatching(')', message).Trim();
        _context.ReadSymbol(")", message);
        var openBrace = _context.ReadSymbol("{", message);
        var children = ParseBlockChildren(message);
        return new IfBlockNode(
            conditionExpression,
            new TextSpan(conditionStart, Math.Max(conditionExpression.Length, 0)),
            children,
            new TextSpan(start, openBrace.Span.End - start));
    }

    private ChildNode ParseMarkupNode(string message)
    {
        var openAngle = _context.ReadSymbol("<", message);
        if (_context.TryReadSymbol("/"))
        {
            throw _context.CreateException(openAngle.Span, "unsupported tag name");
        }

        var tagName = ParseTagName();
        var properties = ParseProperties(tagName.Text);
        if (IsDefaultSlotTag(tagName))
        {
            return _context.TryReadSymbol("/")
                ? ParseSelfClosingSlotOutlet(openAngle, properties)
                : ParseElementSlotOutlet(openAngle, properties);
        }

        return _context.TryReadSymbol("/")
            ? ParseSelfClosingNode(openAngle, tagName, properties)
            : ParseElementNode(openAngle, tagName, properties);
    }

    private PropertyNode ParseProperty()
    {
        var (name, ownerName, propertyName, nameSpan) = ParsePropertyName();
        _context.ReadSymbol("=", $"unsupported prop name '{name}'");

        if (_context.TryReadSymbol("{"))
        {
            var valueStart = _context.Current.Span.Start;
            var value = _context.ReadRawUntilMatching('}', $"unsupported prop name '{name}'").Trim();
            var closeBrace = _context.ReadSymbol("}", $"unsupported prop name '{name}'");
            return new PropertyNode(
                name,
                ownerName,
                propertyName,
                PropertyValueKind.Expression,
                value,
                new TextSpan(valueStart, Math.Max(value.Length, 0)),
                new TextSpan(nameSpan.Start, closeBrace.Span.End - nameSpan.Start));
        }

        var stringToken = _context.ReadStringLiteral($"unsupported prop name '{name}'");
        return new PropertyNode(
            name,
            ownerName,
            propertyName,
            PropertyValueKind.StringLiteral,
            stringToken.Text,
            stringToken.Span,
            new TextSpan(nameSpan.Start, stringToken.Span.End - nameSpan.Start));
    }

    private IReadOnlyList<PropertyNode> ParseProperties(string tagName)
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
        MarkupTagName tagName,
        IReadOnlyList<PropertyNode> properties)
    {
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName,
            properties,
            Array.Empty<ChildNode>(),
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private MarkupNode ParseElementNode(
        Token openAngle,
        MarkupTagName tagName,
        IReadOnlyList<PropertyNode> properties)
    {
        _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        var children = new List<ChildNode>();
        while (!IsClosingTag(tagName))
        {
            children.Add(ParseChildNode($"unsupported tag name '{tagName.Text}'"));
        }

        _context.ReadSymbol("<", $"unsupported tag name '{tagName.Text}'");
        _context.ReadSymbol("/", $"unsupported tag name '{tagName.Text}'");
        ReadMatchingClosingTag(tagName);
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName,
            properties,
            children,
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private SlotOutletNode ParseSelfClosingSlotOutlet(
        Token openAngle,
        IReadOnlyList<PropertyNode> properties)
    {
        var closeAngle = _context.ReadSymbol(">", "unsupported tag name 'Slot'");
        return new SlotOutletNode(
            properties,
            Array.Empty<ChildNode>(),
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private SlotOutletNode ParseElementSlotOutlet(
        Token openAngle,
        IReadOnlyList<PropertyNode> properties)
    {
        _context.ReadSymbol(">", "unsupported tag name 'Slot'");
        var children = new List<ChildNode>();
        while (!IsClosingTag(new MarkupTagName("Slot", null, "Slot", new TextSpan(openAngle.Span.Start, 4))))
        {
            children.Add(ParseChildNode("unsupported tag name 'Slot'"));
        }

        _context.ReadSymbol("<", "unsupported tag name 'Slot'");
        _context.ReadSymbol("/", "unsupported tag name 'Slot'");
        _context.ReadIdentifier("Slot", "unsupported tag name 'Slot'");
        var closeAngle = _context.ReadSymbol(">", "unsupported tag name 'Slot'");
        return new SlotOutletNode(
            properties,
            children,
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private bool IsClosingTag(MarkupTagName tagName)
    {
        if (!_context.PeekSymbol("<") || !_context.PeekSymbol("/", 1))
        {
            return false;
        }

        if (tagName.Prefix is null)
        {
            return _context.PeekIdentifier(tagName.LocalName, 2) &&
                !_context.PeekSymbol(":", 3);
        }

        return _context.PeekIdentifier(tagName.Prefix, 2) &&
            _context.PeekSymbol(":", 3) &&
            _context.PeekIdentifier(tagName.LocalName, 4);
    }

    private MarkupTagName ParseTagName()
    {
        var firstPart = _context.ReadIdentifier("unsupported tag name");
        if (!_context.TryReadSymbol(":"))
        {
            return new MarkupTagName(
                firstPart.Text,
                null,
                firstPart.Text,
                firstPart.Span);
        }

        var localName = _context.ReadIdentifier("unsupported tag name");
        return new MarkupTagName(
            $"{firstPart.Text}:{localName.Text}",
            firstPart.Text,
            localName.Text,
            new TextSpan(firstPart.Span.Start, localName.Span.End - firstPart.Span.Start));
    }

    private void ReadMatchingClosingTag(MarkupTagName tagName)
    {
        if (tagName.Prefix is not null)
        {
            _context.ReadIdentifier(tagName.Prefix, $"unsupported tag name '{tagName.Text}'");
            _context.ReadSymbol(":", $"unsupported tag name '{tagName.Text}'");
        }

        _context.ReadIdentifier(tagName.LocalName, $"unsupported tag name '{tagName.Text}'");
    }

    private static bool IsDefaultSlotTag(MarkupTagName tagName)
    {
        return tagName.Prefix is null &&
               string.Equals(tagName.LocalName, "Slot", StringComparison.Ordinal);
    }

    private (string Name, string? OwnerName, string PropertyName, TextSpan Span) ParsePropertyName()
    {
        var firstPart = _context.ReadIdentifier("unsupported prop name");
        if (!_context.TryReadSymbol("."))
        {
            return (firstPart.Text, null, firstPart.Text, firstPart.Span);
        }

        var propertyPart = _context.ReadIdentifier("unsupported prop name");
        return (
            $"{firstPart.Text}.{propertyPart.Text}",
            firstPart.Text,
            propertyPart.Text,
            new TextSpan(firstPart.Span.Start, propertyPart.Span.End - firstPart.Span.Start));
    }
}
