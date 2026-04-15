using Csxaml.Generator;
using Csxaml.Tooling.Core.Markup;

namespace Csxaml.Tooling.Core.CSharp;

internal static class CsxamlExpressionSpanLocator
{
    public static TextSpan GetForEachCollectionSpan(SourceDocument source, ForEachBlockNode node)
    {
        var text = source.Text;
        var openParen = text.IndexOf('(', node.Span.Start, node.Span.Length);
        if (openParen < 0)
        {
            throw new InvalidOperationException("foreach projection requires an opening parenthesis.");
        }

        var closeParen = CsxamlTextScanner.FindMatchingDelimiter(text, openParen, '(', ')');
        if (closeParen < 0)
        {
            throw new InvalidOperationException("foreach projection requires a closing parenthesis.");
        }

        var inStart = FindTopLevelKeyword(source, openParen + 1, closeParen, "in");
        var expressionStart = CSharpTextScanner.SkipWhitespaceAndComments(source, inStart + 2);
        return new TextSpan(expressionStart, closeParen - expressionStart);
    }

    public static TextSpan GetIfConditionSpan(SourceDocument source, IfBlockNode node)
    {
        var text = source.Text;
        var openParen = text.IndexOf('(', node.Span.Start, node.Span.Length);
        if (openParen < 0)
        {
            throw new InvalidOperationException("if projection requires an opening parenthesis.");
        }

        var closeParen = CsxamlTextScanner.FindMatchingDelimiter(text, openParen, '(', ')');
        if (closeParen < 0)
        {
            throw new InvalidOperationException("if projection requires a closing parenthesis.");
        }

        return new TextSpan(openParen + 1, closeParen - openParen - 1);
    }

    public static TextSpan GetPropertyExpressionSpan(SourceDocument source, PropertyNode property)
    {
        var text = source.Text;
        var searchStart = CSharpTextScanner.SkipWhitespaceAndComments(source, property.Span.Start);
        var equalsIndex = text.IndexOf('=', searchStart, property.Span.Length);
        if (equalsIndex < 0)
        {
            throw new InvalidOperationException("Property projection requires an equals sign.");
        }

        var valueStart = CSharpTextScanner.SkipWhitespaceAndComments(source, equalsIndex + 1);
        if (valueStart >= text.Length || text[valueStart] != '{')
        {
            throw new InvalidOperationException("Property projection requires an expression value.");
        }

        var closeBrace = CsxamlTextScanner.FindMatchingDelimiter(text, valueStart, '{', '}');
        if (closeBrace < 0)
        {
            throw new InvalidOperationException("Property projection requires a closing brace.");
        }

        return new TextSpan(valueStart + 1, closeBrace - valueStart - 1);
    }

    private static int FindTopLevelKeyword(SourceDocument source, int start, int end, string keyword)
    {
        var text = source.Text;
        var parenDepth = 0;
        var bracketDepth = 0;
        var braceDepth = 0;

        for (var index = start; index < end; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            switch (text[index])
            {
                case '(':
                    parenDepth++;
                    continue;
                case ')':
                    parenDepth--;
                    continue;
                case '[':
                    bracketDepth++;
                    continue;
                case ']':
                    bracketDepth--;
                    continue;
                case '{':
                    braceDepth++;
                    continue;
                case '}':
                    braceDepth--;
                    continue;
            }

            if (parenDepth != 0 || bracketDepth != 0 || braceDepth != 0)
            {
                continue;
            }

            if (!CSharpTextScanner.IdentifierEqualsAt(source, index, keyword, out _))
            {
                continue;
            }

            return index;
        }

        throw new InvalidOperationException($"Unable to locate '{keyword}' in projected expression.");
    }
}
