using System.Text.Json;
using Csxaml.LanguageServer.Protocol;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private object HandleCodeAction(JsonElement root)
    {
        var @params = root.GetProperty("params");
        var uri = @params.GetProperty("textDocument").GetProperty("uri").GetString()!;
        var filePath = LspDocumentUriConverter.ToFilePath(uri);
        var range = @params.GetProperty("range");
        var start = range.GetProperty("start");
        var end = range.GetProperty("end");
        var text = _documents.GetOrLoad(uri);

        var actions = _codeActionService.GetCodeActions(
            filePath,
            text,
            start.GetProperty("line").GetInt32(),
            start.GetProperty("character").GetInt32(),
            end.GetProperty("line").GetInt32(),
            end.GetProperty("character").GetInt32());

        return actions.Select(
            action => new
            {
                title = action.Title,
                kind = "quickfix",
                isPreferred = true,
                edit = new
                {
                    changes = new Dictionary<string, object[]>
                    {
                        [uri] = action.Edits
                            .Select(
                                edit => (object)new
                                {
                                    range = new
                                    {
                                        start = new { line = edit.StartLine, character = edit.StartCharacter },
                                        end = new { line = edit.EndLine, character = edit.EndCharacter },
                                    },
                                    newText = edit.NewText,
                                })
                            .ToArray(),
                    },
                },
            })
            .ToArray();
    }
}
