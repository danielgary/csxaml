namespace Csxaml.Tooling.Core.Text;

internal static class CsxamlTextCoordinateConverter
{
    public static (int Line, int Character) GetLineAndCharacter(string text, int offset)
    {
        var boundedOffset = Math.Clamp(offset, 0, text.Length);
        var line = 0;
        var character = 0;

        for (var index = 0; index < boundedOffset; index++)
        {
            if (text[index] != '\n')
            {
                character++;
                continue;
            }

            line++;
            character = 0;
        }

        return (line, character);
    }
}
