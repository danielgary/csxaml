using System.Globalization;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class GridLengthParser
{
    public static IReadOnlyList<GridLength> Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<GridLength>();
        }

        return text
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseToken)
            .ToArray();
    }

    private static GridLength ParseToken(string token)
    {
        if (string.Equals(token, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            return GridLength.Auto;
        }

        if (token.EndsWith('*'))
        {
            var factorText = token[..^1].Trim();
            var factor = string.IsNullOrEmpty(factorText)
                ? 1d
                : double.Parse(factorText, CultureInfo.InvariantCulture);
            return new GridLength(factor, GridUnitType.Star);
        }

        return new GridLength(double.Parse(token, CultureInfo.InvariantCulture), GridUnitType.Pixel);
    }
}
