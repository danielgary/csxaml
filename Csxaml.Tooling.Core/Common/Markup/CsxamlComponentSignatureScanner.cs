using System.Text.RegularExpressions;

namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Scans CSXAML source text for component declarations and parameter signatures.
/// </summary>
public static partial class CsxamlComponentSignatureScanner
{
    /// <summary>
    /// Scans source text for component declarations.
    /// </summary>
    /// <param name="text">The CSXAML source text to scan.</param>
    /// <returns>The component signatures discovered in the source text.</returns>
    public static IReadOnlyList<CsxamlComponentSignature> Scan(string text)
    {
        var declarations = new List<CsxamlComponentSignature>();
        foreach (Match match in ComponentDeclarationPattern().Matches(text))
        {
            var nameGroup = match.Groups["name"];
            var parameters = ReadParameters(text, nameGroup.Index + nameGroup.Length);
            declarations.Add(
                new CsxamlComponentSignature(
                    nameGroup.Value,
                    nameGroup.Index,
                    nameGroup.Length,
                    parameters));
        }

        return declarations;
    }

    private static IReadOnlyList<CsxamlComponentParameterSignature> ReadParameters(
        string text,
        int startIndex)
    {
        var index = CsxamlTextScanner.SkipWhitespace(text, startIndex);
        if (index >= text.Length || text[index] != '(')
        {
            return Array.Empty<CsxamlComponentParameterSignature>();
        }

        var closeParenIndex = CsxamlTextScanner.FindMatchingDelimiter(text, index, '(', ')');
        if (closeParenIndex < 0)
        {
            return Array.Empty<CsxamlComponentParameterSignature>();
        }

        var parameterText = text[(index + 1)..closeParenIndex];
        return CsxamlTextScanner.SplitTopLevelRanges(parameterText, index + 1, ',')
            .Select(CreateParameter)
            .Where(parameter => parameter is not null)
            .Cast<CsxamlComponentParameterSignature>()
            .ToList();
    }

    private static CsxamlComponentParameterSignature? CreateParameter(TopLevelTextSegment segment)
    {
        var match = ParameterNamePattern().Match(segment.Text);
        if (!match.Success)
        {
            return null;
        }

        var nameGroup = match.Groups["name"];
        var typeName = segment.Text[..nameGroup.Index].Trim();
        var trailingWhitespaceLength = segment.Text.Length - segment.Text.TrimEnd().Length;
        var start = segment.End - trailingWhitespaceLength - nameGroup.Length;

        return new CsxamlComponentParameterSignature(
            nameGroup.Value,
            typeName,
            start,
            nameGroup.Length);
    }

    [GeneratedRegex(
        @"\bcomponent\s+Element\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.CultureInvariant)]
    private static partial Regex ComponentDeclarationPattern();

    [GeneratedRegex(
        @"(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex ParameterNamePattern();
}
