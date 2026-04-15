namespace Csxaml.Generator;

internal static class SourceTextCoordinateConverter
{
    public static SourceTextCoordinate GetLineAndColumn(string text, int position)
    {
        var line = 1;
        var column = 1;

        for (var index = 0; index < position && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                column = 1;
                continue;
            }

            column++;
        }

        return new SourceTextCoordinate(line, column);
    }
}
