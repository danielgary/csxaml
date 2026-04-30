namespace Csxaml.Generator;

internal sealed class MarkupNodeParser
{
    private readonly ParserContext _context;
    private readonly Func<string, bool, ChildNode> _parseChildNode;
    private readonly MarkupTagParser _tagParser;

    public MarkupNodeParser(ParserContext context, Func<string, bool, ChildNode> parseChildNode)
    {
        _context = context;
        _parseChildNode = parseChildNode;
        _tagParser = new MarkupTagParser(context);
    }

    public ChildNode ParseMarkupNode(string message, bool allowPropertyContent)
    {
        var openAngle = _context.ReadSymbol("<", message);
        if (_context.TryReadSymbol("/"))
        {
            throw _context.CreateException(openAngle.Span, "unsupported tag name");
        }

        var tagName = _tagParser.ParseTagName();
        var attributes = ParseAttributes();
        if (allowPropertyContent &&
            PropertyContentName.TrySplit(tagName.Text, out var ownerName, out var propertyName))
        {
            return _context.TryReadSymbol("/")
                ? ParseSelfClosingPropertyContent(openAngle, tagName, ownerName, propertyName, attributes)
                : ParseElementPropertyContent(openAngle, tagName, ownerName, propertyName, attributes);
        }

        if (IsDefaultSlotTag(tagName))
        {
            RejectSlotRef(attributes);
            return _context.TryReadSymbol("/")
                ? ParseSelfClosingSlotOutlet(openAngle, attributes.Properties)
                : ParseElementSlotOutlet(openAngle, attributes.Properties);
        }

        return _context.TryReadSymbol("/")
            ? ParseSelfClosingNode(openAngle, tagName, attributes)
            : ParseElementNode(openAngle, tagName, attributes);
    }

    private MarkupAttributeSet ParseAttributes()
    {
        var properties = new List<PropertyNode>();
        ElementRefNode? refNode = null;
        while (!_context.PeekSymbol(">") &&
               !(_context.PeekSymbol("/") && _context.PeekSymbol(">", 1)))
        {
            var property = ParseProperty();
            if (!IsRefAttribute(property))
            {
                properties.Add(property);
                continue;
            }

            if (refNode is not null)
            {
                throw _context.CreateException(
                    property.Span,
                    "duplicate attribute name 'Ref'");
            }

            refNode = new ElementRefNode(
                property.ValueKind,
                property.ValueText,
                property.ValueSpan,
                property.Span);
        }

        return new MarkupAttributeSet(properties, refNode);
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

    private MarkupNode ParseSelfClosingNode(
        Token openAngle,
        MarkupTagName tagName,
        MarkupAttributeSet attributes)
    {
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName,
            attributes.Properties,
            attributes.Ref,
            Array.Empty<ChildNode>(),
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private MarkupNode ParseElementNode(
        Token openAngle,
        MarkupTagName tagName,
        MarkupAttributeSet attributes)
    {
        _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        var children = new List<ChildNode>();
        var propertyContent = new List<PropertyContentNode>();
        while (!_tagParser.IsClosingTag(tagName))
        {
            var child = _parseChildNode($"unsupported tag name '{tagName.Text}'", true);
            if (child is PropertyContentNode propertyContentNode)
            {
                propertyContent.Add(propertyContentNode);
                continue;
            }

            children.Add(child);
        }

        _context.ReadSymbol("<", $"unsupported tag name '{tagName.Text}'");
        _context.ReadSymbol("/", $"unsupported tag name '{tagName.Text}'");
        _tagParser.ReadMatchingClosingTag(tagName);
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new MarkupNode(
            tagName,
            attributes.Properties,
            attributes.Ref,
            propertyContent,
            children,
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private PropertyContentNode ParseSelfClosingPropertyContent(
        Token openAngle,
        MarkupTagName tagName,
        string ownerName,
        string propertyName,
        MarkupAttributeSet attributes)
    {
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new PropertyContentNode(
            ownerName,
            propertyName,
            attributes.Properties,
            attributes.Ref,
            Array.Empty<ChildNode>(),
            new TextSpan(openAngle.Span.Start, closeAngle.Span.End - openAngle.Span.Start));
    }

    private PropertyContentNode ParseElementPropertyContent(
        Token openAngle,
        MarkupTagName tagName,
        string ownerName,
        string propertyName,
        MarkupAttributeSet attributes)
    {
        _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        var children = new List<ChildNode>();
        while (!_tagParser.IsClosingTag(tagName))
        {
            children.Add(_parseChildNode($"unsupported tag name '{tagName.Text}'", true));
        }

        _context.ReadSymbol("<", $"unsupported tag name '{tagName.Text}'");
        _context.ReadSymbol("/", $"unsupported tag name '{tagName.Text}'");
        _tagParser.ReadMatchingClosingTag(tagName);
        var closeAngle = _context.ReadSymbol(">", $"unsupported tag name '{tagName.Text}'");
        return new PropertyContentNode(
            ownerName,
            propertyName,
            attributes.Properties,
            attributes.Ref,
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
        var slotTag = new MarkupTagName("Slot", null, "Slot", new TextSpan(openAngle.Span.Start, 4));
        while (!_tagParser.IsClosingTag(slotTag))
        {
            children.Add(_parseChildNode("unsupported tag name 'Slot'", true));
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

    private static bool IsDefaultSlotTag(MarkupTagName tagName)
    {
        return tagName.Prefix is null &&
               string.Equals(tagName.LocalName, "Slot", StringComparison.Ordinal);
    }

    private static bool IsRefAttribute(PropertyNode property)
    {
        return !property.IsAttached &&
               string.Equals(property.Name, "Ref", StringComparison.Ordinal);
    }

    private void RejectSlotRef(MarkupAttributeSet attributes)
    {
        if (attributes.Ref is not null)
        {
            throw _context.CreateException(
                attributes.Ref.Span,
                "Ref is only supported on native controls");
        }
    }

    private sealed record MarkupAttributeSet(
        IReadOnlyList<PropertyNode> Properties,
        ElementRefNode? Ref);
}
