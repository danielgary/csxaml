namespace Csxaml.Generator;

internal static class DiagnosticMessageFormatter
{
    public static string WithSuggestion(
        string message,
        string actualName,
        IEnumerable<string> candidates)
    {
        var suggestion = ClosestNameSuggester.Find(actualName, candidates);
        return suggestion is null
            ? message
            : $"{message}; did you mean '{suggestion}'?";
    }
}
