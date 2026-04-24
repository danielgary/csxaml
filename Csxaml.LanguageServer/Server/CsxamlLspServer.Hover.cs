using System.Text.Json;
using Csxaml.LanguageServer.Documents;
using Csxaml.LanguageServer.Protocol;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private object? HandleHover(JsonElement root)
    {
        var @params = root.GetProperty("params");
        var uri = @params.GetProperty("textDocument").GetProperty("uri").GetString()!;
        var filePath = LspDocumentUriConverter.ToFilePath(uri);
        var position = @params.GetProperty("position");
        var text = _documents.GetOrLoad(uri);
        var offset = LspTextPositionConverter.GetOffset(
            text,
            position.GetProperty("line").GetInt32(),
            position.GetProperty("character").GetInt32());

        var hover = _hoverService.GetHover(filePath, text, offset);
        if (hover is null)
        {
            return null;
        }

        var start = LspTextPositionConverter.GetLineAndCharacter(text, hover.Start);
        var end = LspTextPositionConverter.GetLineAndCharacter(text, hover.Start + hover.Length);
        return new
        {
            contents = new
            {
                kind = "markdown",
                value = hover.Markdown,
            },
            range = new
            {
                start = new { line = start.Line, character = start.Character },
                end = new { line = end.Line, character = end.Character },
            },
        };
    }
}
