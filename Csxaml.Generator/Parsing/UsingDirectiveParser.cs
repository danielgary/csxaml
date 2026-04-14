namespace Csxaml.Generator;

internal sealed class UsingDirectiveParser
{
    private readonly ParserContext _context;

    public UsingDirectiveParser(ParserContext context)
    {
        _context = context;
    }

    public IReadOnlyList<UsingDirectiveDefinition> ParseDirectives()
    {
        var directives = new List<UsingDirectiveDefinition>();
        while (_context.PeekIdentifier("using"))
        {
            directives.Add(ParseDirective());
        }

        return directives;
    }

    private UsingDirectiveDefinition ParseDirective()
    {
        const string message = "invalid using directive";
        var start = _context.ReadIdentifier("using", message).Span.Start;
        var firstToken = _context.ReadIdentifier(message);
        string? alias = null;
        string namespaceName;

        if (_context.TryReadSymbol("="))
        {
            alias = firstToken.Text;
            namespaceName = ReadQualifiedName(message);
        }
        else
        {
            namespaceName = ReadQualifiedName(firstToken, message);
        }

        var semicolon = _context.ReadSymbol(";", message);
        return new UsingDirectiveDefinition(
            alias,
            namespaceName,
            new TextSpan(start, semicolon.Span.End - start));
    }

    private string ReadQualifiedName(string message)
    {
        return ReadQualifiedName(_context.ReadIdentifier(message), message);
    }

    private string ReadQualifiedName(Token firstToken, string message)
    {
        var end = firstToken.Span.End;
        while (_context.TryReadSymbol("."))
        {
            end = _context.ReadIdentifier(message).Span.End;
        }

        return _context.Source.Text[firstToken.Span.Start..end];
    }
}
