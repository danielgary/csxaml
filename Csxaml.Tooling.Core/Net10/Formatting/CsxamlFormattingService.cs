namespace Csxaml.Tooling.Core.Formatting;

/// <summary>
/// Formats CSXAML source text using the editor-facing indentation rules.
/// </summary>
public sealed class CsxamlFormattingService
{
    private const int SpacesPerIndent = 4;

    /// <summary>
    /// Formats an entire CSXAML document.
    /// </summary>
    /// <param name="text">The source text to format.</param>
    /// <returns>The formatted source text.</returns>
    public string FormatDocument(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var formattedLines = new string[lines.Length];
        var csharpIndent = 0;
        var markupIndent = 0;

        for (var index = 0; index < lines.Length; index++)
        {
            var rawLine = lines[index];
            var trimmed = rawLine.Trim();
            if (trimmed.Length == 0)
            {
                formattedLines[index] = string.Empty;
                continue;
            }

            var leadingCsharpClosers = CountLeadingCharacter(trimmed, '}');
            var adjustedCsharpIndent = leadingCsharpClosers > 0
                ? Math.Max(csharpIndent - leadingCsharpClosers, 0)
                : csharpIndent;
            var adjustedMarkupIndent = StartsWithMarkupClose(trimmed)
                ? Math.Max(markupIndent - 1, 0)
                : markupIndent;

            formattedLines[index] = $"{new string(' ', (adjustedCsharpIndent + adjustedMarkupIndent) * SpacesPerIndent)}{trimmed}";

            csharpIndent = UpdateCsharpIndent(trimmed, adjustedCsharpIndent, leadingCsharpClosers);
            markupIndent = UpdateMarkupIndent(trimmed, adjustedMarkupIndent);
        }

        var formatted = string.Join(newline, formattedLines);
        return text.EndsWith(newline, StringComparison.Ordinal) && !formatted.EndsWith(newline, StringComparison.Ordinal)
            ? formatted + newline
            : formatted;
    }

    private static int UpdateCsharpIndent(string trimmedLine, int currentIndent, int leadingClosers)
    {
        if (LooksLikeMarkupOrAttributeLine(trimmedLine))
        {
            return currentIndent;
        }

        var closingBraceCount = Math.Max(CountCharacter(trimmedLine, '}') - leadingClosers, 0);
        return Math.Max(currentIndent + CountCharacter(trimmedLine, '{') - closingBraceCount, 0);
    }

    private static int UpdateMarkupIndent(string trimmedLine, int currentIndent)
    {
        var markupSegment = GetMarkupSegment(trimmedLine);
        if (markupSegment is null ||
            markupSegment.StartsWith("</", StringComparison.Ordinal) ||
            markupSegment.Contains("/>", StringComparison.Ordinal) ||
            markupSegment.Contains("</", StringComparison.Ordinal))
        {
            return currentIndent;
        }

        return currentIndent + 1;
    }

    private static bool StartsWithMarkupClose(string trimmedLine)
    {
        var markupSegment = GetMarkupSegment(trimmedLine);
        return markupSegment?.StartsWith("</", StringComparison.Ordinal) == true;
    }

    private static bool LooksLikeMarkupOrAttributeLine(string trimmedLine)
    {
        if (GetMarkupSegment(trimmedLine) is not null)
        {
            return true;
        }

        if (!trimmedLine.Contains('=', StringComparison.Ordinal))
        {
            return false;
        }

        return !trimmedLine.StartsWith("if ", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("if(", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("foreach ", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("foreach(", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("for ", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("for(", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("while ", StringComparison.Ordinal) &&
            !trimmedLine.StartsWith("while(", StringComparison.Ordinal);
    }

    private static string? GetMarkupSegment(string trimmedLine)
    {
        var tagStart = trimmedLine.IndexOf('<');
        if (tagStart < 0)
        {
            return null;
        }

        if (tagStart == 0)
        {
            return trimmedLine;
        }

        var prefix = trimmedLine[..tagStart].TrimEnd();
        return string.Equals(prefix, "render", StringComparison.Ordinal)
            ? trimmedLine[tagStart..]
            : null;
    }

    private static int CountCharacter(string text, char character)
    {
        var count = 0;
        foreach (var current in text)
        {
            if (current == character)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountLeadingCharacter(string text, char character)
    {
        var count = 0;
        foreach (var current in text)
        {
            if (current != character)
            {
                break;
            }

            count++;
        }

        return count;
    }
}
