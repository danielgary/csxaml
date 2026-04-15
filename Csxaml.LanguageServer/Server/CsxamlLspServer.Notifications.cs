using System.Text.Json;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private async Task HandleNotificationAsync(string method, JsonElement root, CancellationToken cancellationToken)
    {
        switch (method)
        {
            case "exit":
                return;
            case "initialized":
                return;
            case "textDocument/didClose":
                {
                    var uri = root.GetProperty("params").GetProperty("textDocument").GetProperty("uri").GetString()!;
                    _documents.Remove(uri);
                    await PublishDiagnosticsAsync(uri, string.Empty, cancellationToken);
                    return;
                }

            case "textDocument/didOpen":
                {
                    var document = root.GetProperty("params").GetProperty("textDocument");
                    var uri = document.GetProperty("uri").GetString()!;
                    var text = document.GetProperty("text").GetString() ?? string.Empty;
                    _documents.Set(uri, text);
                    await PublishDiagnosticsAsync(uri, text, cancellationToken);
                    return;
                }

            case "textDocument/didChange":
                {
                    var @params = root.GetProperty("params");
                    var uri = @params.GetProperty("textDocument").GetProperty("uri").GetString()!;
                    var text = @params.GetProperty("contentChanges")[0].GetProperty("text").GetString() ?? string.Empty;
                    _documents.Set(uri, text);
                    await PublishDiagnosticsAsync(uri, text, cancellationToken);
                    return;
                }
        }
    }

    private async Task PublishDiagnosticsAsync(string uri, string text, CancellationToken cancellationToken)
    {
        var diagnostics = string.IsNullOrEmpty(text)
            ? Array.Empty<object>()
            : _diagnosticService.GetDiagnostics(new Uri(uri).LocalPath, text)
                .Select(
                    diagnostic => new
                    {
                        range = new
                        {
                            start = new { line = diagnostic.StartLine, character = diagnostic.StartCharacter },
                            end = new { line = diagnostic.EndLine, character = diagnostic.EndCharacter },
                        },
                        severity = 1,
                        source = "csxaml",
                        message = diagnostic.Message,
                    })
                .Cast<object>()
                .ToArray();

        await _writer.WriteNotificationAsync(
            "textDocument/publishDiagnostics",
            new
            {
                uri,
                diagnostics,
            },
            cancellationToken);
    }
}
