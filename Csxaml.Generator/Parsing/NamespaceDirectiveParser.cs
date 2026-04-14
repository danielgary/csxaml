namespace Csxaml.Generator;

internal sealed class NamespaceDirectiveParser
{
    private readonly ParserContext _context;

    public NamespaceDirectiveParser(ParserContext context)
    {
        _context = context;
    }

    public FileScopedNamespaceDefinition? ParseDirective()
    {
        const string message = "invalid file-scoped namespace declaration";
        if (!_context.PeekIdentifier("namespace"))
        {
            return null;
        }

        var start = _context.ReadIdentifier("namespace", message).Span.Start;
        var firstPart = _context.ReadIdentifier(message);
        var end = firstPart.Span.End;

        while (_context.TryReadSymbol("."))
        {
            end = _context.ReadIdentifier(message).Span.End;
        }

        var semicolon = _context.ReadSymbol(";", message);
        return new FileScopedNamespaceDefinition(
            _context.Source.Text[firstPart.Span.Start..end],
            new TextSpan(start, semicolon.Span.End - start));
    }
}
