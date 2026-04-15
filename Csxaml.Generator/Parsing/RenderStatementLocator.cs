namespace Csxaml.Generator;

internal sealed class RenderStatementLocator
{
    private const string MissingRenderStatement = "missing final render statement in component body";
    private const string InvalidReturnStatement = "component final markup must use 'render <Root />;'";
    private const string InvalidRenderPayload = "render statement must contain a single markup root";
    private readonly SourceDocument _source;

    public RenderStatementLocator(SourceDocument source)
    {
        _source = source;
    }

    public int Locate(int startPosition)
    {
        var text = _source.Text;
        var braceDepth = 0;
        var parenDepth = 0;
        var bracketDepth = 0;

        for (var index = startPosition; index < text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(_source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = text[index];
            switch (current)
            {
                case '{':
                    braceDepth++;
                    break;

                case '}':
                    if (braceDepth == 0 && parenDepth == 0 && bracketDepth == 0)
                    {
                        throw DiagnosticFactory.FromPosition(_source, index, MissingRenderStatement);
                    }

                    braceDepth--;
                    break;

                case '(':
                    parenDepth++;
                    break;

                case ')':
                    parenDepth--;
                    break;

                case '[':
                    bracketDepth++;
                    break;

                case ']':
                    bracketDepth--;
                    break;
            }

            if (braceDepth != 0 || parenDepth != 0 || bracketDepth != 0)
            {
                continue;
            }

            if (CSharpTextScanner.IdentifierEqualsAt(_source, index, "return", out _))
            {
                throw DiagnosticFactory.FromPosition(_source, index, InvalidReturnStatement);
            }

            if (!CSharpTextScanner.IdentifierEqualsAt(_source, index, "render", out var identifierEnd))
            {
                continue;
            }

            var contentStart = CSharpTextScanner.SkipWhitespaceAndComments(_source, identifierEnd);
            if (contentStart >= text.Length || text[contentStart] != '<')
            {
                throw DiagnosticFactory.FromPosition(_source, index, InvalidRenderPayload);
            }

            return index;
        }

        throw DiagnosticFactory.FromPosition(_source, startPosition, MissingRenderStatement);
    }
}
