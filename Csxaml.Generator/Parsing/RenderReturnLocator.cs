namespace Csxaml.Generator;

internal sealed class RenderReturnLocator
{
    private readonly SourceDocument _source;

    public RenderReturnLocator(SourceDocument source)
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
                        throw DiagnosticFactory.FromPosition(_source, index, "missing return block");
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

            if (!CSharpTextScanner.IdentifierEqualsAt(_source, index, "return", out var identifierEnd))
            {
                continue;
            }

            var contentStart = CSharpTextScanner.SkipWhitespaceAndComments(_source, identifierEnd);
            if (contentStart < text.Length && text[contentStart] == '<')
            {
                return index;
            }

            index = identifierEnd - 1;
        }

        throw DiagnosticFactory.FromPosition(_source, startPosition, "missing return block");
    }
}
