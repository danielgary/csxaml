namespace Csxaml.LanguageServer.Documents;

internal static class LspTextPositionConverter
{
    public static int GetOffset(string text, int line, int character)
    {
        var currentLine = 0;
        var index = 0;
        while (index < text.Length && currentLine < line)
        {
            if (text[index] == '\n')
            {
                currentLine++;
            }

            index++;
        }

        return Math.Min(index + character, text.Length);
    }

    public static (int Line, int Character) GetLineAndCharacter(string text, int offset)
    {
        var line = 0;
        var character = 0;
        for (var index = 0; index < offset && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                character = 0;
            }
            else
            {
                character++;
            }
        }

        return (line, character);
    }
}
