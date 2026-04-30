namespace Csxaml.Generator;

internal sealed partial class Parser
{
    private static IReadOnlyList<RootPropertyDeclaration> ParseRootProperties(
        ParserContext context,
        ComponentKind kind)
    {
        if (kind is not (ComponentKind.Page or ComponentKind.Window))
        {
            return Array.Empty<RootPropertyDeclaration>();
        }

        var properties = new List<RootPropertyDeclaration>();
        while (IsRootPropertyStart(context))
        {
            properties.Add(ParseRootProperty(context));
        }

        return properties;
    }

    private static bool IsRootPropertyStart(ParserContext context)
    {
        return context.Current.Kind == TokenKind.Identifier &&
            context.PeekSymbol("=", 1);
    }

    private static RootPropertyDeclaration ParseRootProperty(ParserContext context)
    {
        const string message = "invalid root property declaration";
        var name = context.ReadIdentifier(message);
        context.ReadSymbol("=", message);
        var valueStart = context.Current.Span.Start;
        var valueEnd = FindRootPropertySemicolon(context.Source, valueStart);
        var valueSpan = CSharpTextScanner.TrimWhitespaceSpan(context.Source, valueStart, valueEnd);
        if (valueSpan is null)
        {
            throw context.CreateException(name.Span, message);
        }

        context.SkipToPosition(valueEnd + 1);
        return new RootPropertyDeclaration(
            name.Text,
            context.Source.Text.Substring(valueSpan.Value.Start, valueSpan.Value.Length),
            valueSpan.Value,
            new TextSpan(name.Span.Start, valueEnd + 1 - name.Span.Start));
    }

    private static int FindRootPropertySemicolon(SourceDocument source, int start)
    {
        var braceDepth = 0;
        var parenDepth = 0;
        var bracketDepth = 0;
        for (var index = start; index < source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            var current = source.Text[index];
            switch (current)
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
                case ';' when braceDepth == 0 && parenDepth == 0 && bracketDepth == 0:
                    return index;
            }
        }

        throw DiagnosticFactory.FromPosition(source, start, "missing ';' after root property declaration");
    }
}
