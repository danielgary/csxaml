using Csxaml.LanguageServer.Protocol;

namespace Csxaml.LanguageServer.Documents;

internal sealed class TextDocumentStore
{
    private readonly Dictionary<string, string> _documents = new(StringComparer.OrdinalIgnoreCase);

    public string GetOrLoad(string uri)
    {
        if (_documents.TryGetValue(uri, out var text))
        {
            return text;
        }

        var filePath = LspDocumentUriConverter.ToFilePath(uri);
        text = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        _documents[uri] = text;
        return text;
    }

    public void Remove(string uri)
    {
        _documents.Remove(uri);
    }

    public void Set(string uri, string text)
    {
        _documents[uri] = text;
    }
}
