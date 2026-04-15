namespace Csxaml.Generator;

internal static class ClosestNameSuggester
{
    public static string? Find(string actualName, IEnumerable<string> candidates)
    {
        var distinctCandidates = candidates
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (distinctCandidates.Count == 0)
        {
            return null;
        }

        var best = distinctCandidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Distance = ComputeDistance(actualName, candidate),
            })
            .OrderBy(result => result.Distance)
            .ThenBy(result => result.Candidate, StringComparer.Ordinal)
            .First();

        var threshold = Math.Max(2, actualName.Length / 2);
        return best.Distance <= threshold
            ? best.Candidate
            : null;
    }

    private static int ComputeDistance(string left, string right)
    {
        var table = new int[left.Length + 1, right.Length + 1];
        for (var row = 0; row <= left.Length; row++)
        {
            table[row, 0] = row;
        }

        for (var column = 0; column <= right.Length; column++)
        {
            table[0, column] = column;
        }

        for (var row = 1; row <= left.Length; row++)
        {
            for (var column = 1; column <= right.Length; column++)
            {
                var substitutionCost = char.ToUpperInvariant(left[row - 1]) == char.ToUpperInvariant(right[column - 1])
                    ? 0
                    : 1;
                table[row, column] = Math.Min(
                    Math.Min(
                        table[row - 1, column] + 1,
                        table[row, column - 1] + 1),
                    table[row - 1, column - 1] + substitutionCost);
            }
        }

        return table[left.Length, right.Length];
    }
}
