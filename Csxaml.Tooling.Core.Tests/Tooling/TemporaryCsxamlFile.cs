namespace Csxaml.Tooling.Core.Tests.Tooling;

internal sealed class TemporaryCsxamlFile : IDisposable
{
    private TemporaryCsxamlFile(string filePath, string text)
    {
        FilePath = filePath;
        Text = text;
    }

    public string FilePath { get; }

    public string Text { get; }

    public static TemporaryCsxamlFile Create(string directory, string text)
    {
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, $"Tooling_{Guid.NewGuid():N}.csxaml");
        File.WriteAllText(filePath, text);
        return new TemporaryCsxamlFile(filePath, text);
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}
