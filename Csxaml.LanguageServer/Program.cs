using Csxaml.LanguageServer.Documents;
using Csxaml.LanguageServer.Protocol;
using Csxaml.LanguageServer.Server;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Definitions;
using Csxaml.Tooling.Core.Diagnostics;
using Csxaml.Tooling.Core.Formatting;
using Csxaml.Tooling.Core.SemanticTokens;

var server = new CsxamlLspServer(
    new LspMessageReader(Console.OpenStandardInput()),
    new LspMessageWriter(Console.OpenStandardOutput()),
    new TextDocumentStore(),
    new CsxamlCompletionService(),
    new CsxamlDefinitionService(),
    new CsxamlDiagnosticService(),
    new CsxamlFormattingService(),
    new CsxamlSemanticTokenService());

await server.RunAsync(CancellationToken.None);
