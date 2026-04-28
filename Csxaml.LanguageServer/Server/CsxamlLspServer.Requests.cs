using System.Text.Json;
using Csxaml.LanguageServer.Documents;
using Csxaml.LanguageServer.Protocol;
using Csxaml.Tooling.Core.Completion;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private async Task HandleRequestAsync(
        string method,
        JsonElement id,
        JsonElement root,
        CancellationToken cancellationToken)
    {
        switch (method)
        {
            case "initialize":
                await _writer.WriteResponseAsync(id, CreateInitializeResult(), cancellationToken);
                return;

            case "shutdown":
                _shutdownRequested = true;
                await _writer.WriteResponseAsync(id, result: null, cancellationToken);
                return;

            case "textDocument/completion":
                await _writer.WriteResponseAsync(id, HandleCompletion(root), cancellationToken);
                return;

            case "textDocument/hover":
                await _writer.WriteResponseAsync(id, HandleHover(root), cancellationToken);
                return;

            case "textDocument/codeAction":
                await _writer.WriteResponseAsync(id, HandleCodeAction(root), cancellationToken);
                return;

            case "textDocument/definition":
                await _writer.WriteResponseAsync(id, HandleDefinition(root), cancellationToken);
                return;

            case "textDocument/semanticTokens/full":
                await _writer.WriteResponseAsync(id, HandleSemanticTokens(root), cancellationToken);
                return;

            case "textDocument/formatting":
                await _writer.WriteResponseAsync(id, HandleFormatting(root), cancellationToken);
                return;

            default:
                await _writer.WriteErrorAsync(id, -32601, $"Method '{method}' is not supported.", cancellationToken);
                return;
        }
    }

    private object CreateInitializeResult()
    {
        return new
        {
            capabilities = new
            {
                textDocumentSync = 1,
                completionProvider = new
                {
                    resolveProvider = false,
                    triggerCharacters = new[] { "<", ":", " ", ".", "{" },
                },
                hoverProvider = true,
                codeActionProvider = new
                {
                    codeActionKinds = new[] { "quickfix" },
                },
                definitionProvider = true,
                documentFormattingProvider = true,
                semanticTokensProvider = new
                {
                    legend = new
                    {
                        tokenTypes = TokenTypes,
                        tokenModifiers = TokenModifiers,
                    },
                    full = true,
                },
            },
            serverInfo = new
            {
                name = "CSXAML Language Server",
                version = typeof(CsxamlLspServer).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            },
        };
    }

    private object HandleCompletion(JsonElement root)
    {
        var @params = root.GetProperty("params");
        var textDocument = @params.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var filePath = LspDocumentUriConverter.ToFilePath(uri);
        var position = @params.GetProperty("position");
        var text = _documents.GetOrLoad(uri);
        var offset = LspTextPositionConverter.GetOffset(
            text,
            position.GetProperty("line").GetInt32(),
            position.GetProperty("character").GetInt32());

        var items = _completionService.GetCompletions(filePath, text, offset)
            .Select(
                item => new
                {
                    label = item.Label,
                    kind = MapCompletionKind(item.Kind),
                    detail = item.Detail,
                    documentation = item.Documentation,
                    insertText = item.InsertText,
                    insertTextFormat = item.IsSnippet ? 2 : 1,
                    sortText = item.SortText,
                })
            .ToList();

        return new
        {
            isIncomplete = false,
            items,
        };
    }

    private object? HandleDefinition(JsonElement root)
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

        var definition = _definitionService.GetDefinition(filePath, text, offset);
        if (definition is null)
        {
            return null;
        }

        var targetText = File.ReadAllText(definition.FilePath);
        var start = LspTextPositionConverter.GetLineAndCharacter(targetText, definition.Start);
        var end = LspTextPositionConverter.GetLineAndCharacter(targetText, definition.Start + definition.Length);
        return new[]
        {
            new
            {
                uri = LspDocumentUriConverter.ToDocumentUri(definition.FilePath),
                range = new
                {
                    start = new { line = start.Line, character = start.Character },
                    end = new { line = end.Line, character = end.Character },
                },
            },
        };
    }

    private object HandleFormatting(JsonElement root)
    {
        var uri = root.GetProperty("params").GetProperty("textDocument").GetProperty("uri").GetString()!;
        var text = _documents.GetOrLoad(uri);
        var formatted = _formattingService.FormatDocument(text);
        if (string.Equals(text, formatted, StringComparison.Ordinal))
        {
            return Array.Empty<object>();
        }

        var end = LspTextPositionConverter.GetLineAndCharacter(text, text.Length);
        return new[]
        {
            new
            {
                range = new
                {
                    start = new { line = 0, character = 0 },
                    end = new { line = end.Line, character = end.Character },
                },
                newText = formatted,
            },
        };
    }

    private static int MapCompletionKind(CsxamlCompletionItemKind kind)
    {
        return kind switch
        {
            CsxamlCompletionItemKind.Method => 2,
            CsxamlCompletionItemKind.Variable => 6,
            CsxamlCompletionItemKind.Class => 7,
            CsxamlCompletionItemKind.Namespace => 9,
            CsxamlCompletionItemKind.Property => 10,
            CsxamlCompletionItemKind.Keyword => 14,
            CsxamlCompletionItemKind.Event => 23,
            CsxamlCompletionItemKind.Parameter => 6,
            _ => 1,
        };
    }
}
