namespace Csxaml.Generator;

internal static class MisplacedComponentPrologueDetector
{
    public static void Validate(
        SourceDocument source,
        ComponentHelperCodeBlock? helperCode)
    {
        if (helperCode is null)
        {
            return;
        }

        var depth = 0;
        var start = helperCode.Span.Start;
        var end = helperCode.Span.End;
        for (var index = start; index < end; index++)
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
                    depth++;
                    continue;
                case '}':
                    depth--;
                    continue;
            }

            if (depth != 0 ||
                !TryMatchIdentifier(source, helperCode, index, "inject") &&
                !TryMatchIdentifier(source, helperCode, index, "State"))
            {
                continue;
            }

            var declaration = source.Text.AsSpan(index).StartsWith("inject".AsSpan(), StringComparison.Ordinal)
                ? "inject"
                : "State";
            throw DiagnosticFactory.FromPosition(
                source,
                index,
                $"{declaration} declarations must appear before helper code");
        }
    }

    private static bool TryMatchIdentifier(
        SourceDocument source,
        ComponentHelperCodeBlock helperCode,
        int index,
        string identifier)
    {
        if (!CSharpTextScanner.IdentifierEqualsAt(source, index, identifier, out var end))
        {
            return false;
        }

        return index >= helperCode.Span.Start && end <= helperCode.Span.End;
    }
}
