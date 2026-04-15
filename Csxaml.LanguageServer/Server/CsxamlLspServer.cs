using System.Text.Json;
using Csxaml.LanguageServer.Documents;
using Csxaml.LanguageServer.Protocol;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Definitions;
using Csxaml.Tooling.Core.Diagnostics;
using Csxaml.Tooling.Core.Formatting;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private readonly CsxamlCompletionService _completionService;
    private readonly CsxamlDefinitionService _definitionService;
    private readonly CsxamlDiagnosticService _diagnosticService;
    private readonly CsxamlFormattingService _formattingService;
    private readonly TextDocumentStore _documents;
    private readonly LspMessageReader _reader;
    private readonly CsxamlSemanticTokenService _semanticTokenService;
    private readonly LspMessageWriter _writer;
    private bool _shutdownRequested;

    public CsxamlLspServer(
        LspMessageReader reader,
        LspMessageWriter writer,
        TextDocumentStore documents,
        CsxamlCompletionService completionService,
        CsxamlDefinitionService definitionService,
        CsxamlDiagnosticService diagnosticService,
        CsxamlFormattingService formattingService,
        CsxamlSemanticTokenService semanticTokenService)
    {
        _reader = reader;
        _writer = writer;
        _documents = documents;
        _completionService = completionService;
        _definitionService = definitionService;
        _diagnosticService = diagnosticService;
        _formattingService = formattingService;
        _semanticTokenService = semanticTokenService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (await _reader.ReadAsync(cancellationToken) is { } message)
        {
            var root = message.RootElement;
            var method = root.TryGetProperty("method", out var methodElement)
                ? methodElement.GetString()
                : null;
            if (method is null)
            {
                continue;
            }

            var hasId = root.TryGetProperty("id", out var id);
            try
            {
                if (hasId)
                {
                    await HandleRequestAsync(method, id, root, cancellationToken);
                }
                else
                {
                    await HandleNotificationAsync(method, root, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                if (hasId)
                {
                    await _writer.WriteErrorAsync(id, -32603, exception.Message, cancellationToken);
                }
            }

            if (_shutdownRequested && method == "exit")
            {
                return;
            }
        }
    }
}
