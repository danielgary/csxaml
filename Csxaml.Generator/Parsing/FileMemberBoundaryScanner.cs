namespace Csxaml.Generator;

internal sealed class FileMemberBoundaryScanner
{
    private readonly SourceDocument _source;

    public FileMemberBoundaryScanner(SourceDocument source)
    {
        _source = source;
    }

    public ComponentFileScanResult Scan(int startPosition)
    {
        var helperBlocks = new List<FileHelperCodeBlock>();
        TextSpan? componentSpan = null;
        var helperStart = startPosition;
        var braceDepth = 0;
        var parenDepth = 0;
        var bracketDepth = 0;

        for (var index = startPosition; index < _source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(_source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            if (braceDepth == 0 &&
                parenDepth == 0 &&
                bracketDepth == 0 &&
                TryMatchComponentDeclaration(index, out var componentStart, out var componentEnd))
            {
                if (componentSpan is not null)
                {
                    throw DiagnosticFactory.FromPosition(
                        _source,
                        componentStart,
                        "only one component per file is supported");
                }

                AddHelperBlock(helperBlocks, helperStart, componentStart);
                componentSpan = new TextSpan(componentStart, componentEnd - componentStart);
                helperStart = componentEnd;
                index = componentEnd - 1;
                continue;
            }

            switch (_source.Text[index])
            {
                case '{':
                    braceDepth++;
                    break;

                case '}':
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
        }

        if (componentSpan is null)
        {
            throw DiagnosticFactory.FromPosition(
                _source,
                startPosition,
                "missing component declaration");
        }

        AddHelperBlock(helperBlocks, helperStart, _source.Text.Length);
        return new ComponentFileScanResult(componentSpan.Value, helperBlocks);
    }

    private bool TryMatchComponentDeclaration(int start, out int componentStart, out int componentEnd)
    {
        componentStart = start;
        componentEnd = start;

        if (!CSharpTextScanner.IdentifierEqualsAt(_source, start, "component", out var afterComponent))
        {
            return false;
        }

        var afterWhitespace = CSharpTextScanner.SkipWhitespaceAndComments(_source, afterComponent);
        if (!CSharpTextScanner.IdentifierEqualsAt(_source, afterWhitespace, "Element", out _))
        {
            return false;
        }

        componentStart = start;
        componentEnd = FindComponentEnd(start);
        return true;
    }

    private int FindComponentEnd(int componentStart)
    {
        var openBrace = FindComponentOpenBrace(componentStart);
        var depth = 1;

        for (var index = openBrace + 1; index < _source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(_source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            if (_source.Text[index] == '{')
            {
                depth++;
                continue;
            }

            if (_source.Text[index] != '}')
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return index + 1;
            }
        }

        throw DiagnosticFactory.FromPosition(
            _source,
            componentStart,
            "invalid component declaration");
    }

    private int FindComponentOpenBrace(int componentStart)
    {
        for (var index = componentStart; index < _source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(_source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            if (_source.Text[index] == '{')
            {
                return index;
            }
        }

        throw DiagnosticFactory.FromPosition(
            _source,
            componentStart,
            "invalid component declaration");
    }

    private void AddHelperBlock(List<FileHelperCodeBlock> helperBlocks, int start, int end)
    {
        var helperSpan = CSharpTextScanner.TrimWhitespaceSpan(_source, start, end);
        if (helperSpan is null)
        {
            return;
        }

        helperBlocks.Add(new FileHelperCodeBlock(
            _source.Text.Substring(helperSpan.Value.Start, helperSpan.Value.Length),
            helperSpan.Value));
    }
}
