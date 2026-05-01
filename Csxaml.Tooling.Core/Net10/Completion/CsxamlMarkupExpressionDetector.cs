namespace Csxaml.Tooling.Core.Completion;

internal static class CsxamlMarkupExpressionDetector
{
    public static bool IsInsideExpression(string text, int position)
    {
        var braceDepth = 0;
        var activeTag = false;
        var inString = false;
        var stringDelimiter = '\0';

        for (var index = 0; index < position && index < text.Length; index++)
        {
            var current = text[index];
            if (inString)
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == stringDelimiter)
                {
                    inString = false;
                }

                continue;
            }

            if (current is '"' or '\'')
            {
                inString = true;
                stringDelimiter = current;
                continue;
            }

            if (current == '<' && braceDepth == 0)
            {
                activeTag = true;
                continue;
            }

            if (!activeTag)
            {
                continue;
            }

            if (current == '{')
            {
                braceDepth++;
                continue;
            }

            if (current == '}' && braceDepth > 0)
            {
                braceDepth--;
                continue;
            }

            if (current == '>' && braceDepth == 0)
            {
                activeTag = false;
            }
        }

        return activeTag && braceDepth > 0;
    }
}
