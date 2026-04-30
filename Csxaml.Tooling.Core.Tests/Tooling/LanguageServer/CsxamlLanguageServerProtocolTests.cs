using System.Text.Json;

namespace Csxaml.Tooling.Core.Tests.Tooling.LanguageServer;

[TestClass]
public sealed class CsxamlLanguageServerProtocolTests
{
    private static readonly string RepoRoot = LanguageServerTestPaths.RepoRoot;

    [TestMethod]
    public async Task Protocol_smoke_path_serves_completion_definition_and_semantic_tokens()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            using Microsoft.UI.Xaml.Automation;

            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
            State<string> SelectedId = new State<string>("todo-1");
            var current = SelectedI;
            render <TodoCard AutomationProperties.Name={SelectedId.Val} />;
            }
            """);

        await using var client = await StartClientAsync();
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        await OpenDocumentAsync(client, documentUri, tempFile.Text);

        var completionPosition = GetLineAndCharacter(
            tempFile.Text,
            tempFile.Text.IndexOf("TodoC", StringComparison.Ordinal) + "TodoC".Length);
        var csharpCompletionPosition = GetLineAndCharacter(
            tempFile.Text,
            tempFile.Text.IndexOf("SelectedI", StringComparison.Ordinal) + "SelectedI".Length);
        var definitionPosition = GetLineAndCharacter(
            tempFile.Text,
            tempFile.Text.IndexOf("TodoCard", StringComparison.Ordinal) + 1);

        var completionResponse = await client.SendRequestAsync(
            "textDocument/completion",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = completionPosition.Line, character = completionPosition.Character },
            },
            CancellationToken.None);
        Assert.IsTrue(
            completionResponse.GetProperty("result").GetProperty("items").EnumerateArray()
                .Any(item => item.GetProperty("label").GetString() == "TodoCard"));

        var csharpCompletionResponse = await client.SendRequestAsync(
            "textDocument/completion",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = csharpCompletionPosition.Line, character = csharpCompletionPosition.Character },
            },
            CancellationToken.None);
        Assert.IsTrue(
            csharpCompletionResponse.GetProperty("result").GetProperty("items").EnumerateArray()
                .Any(item => item.GetProperty("label").GetString() == "SelectedId"));

        var definitionResponse = await client.SendRequestAsync(
            "textDocument/definition",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = definitionPosition.Line, character = definitionPosition.Character },
            },
            CancellationToken.None);
        var definition = definitionResponse.GetProperty("result")[0];
        StringAssert.EndsWith(
            definition.GetProperty("uri").GetString(),
            "/samples/Csxaml.TodoApp/Components/TodoCard.csxaml");

        var tokenResponse = await client.SendRequestAsync(
            "textDocument/semanticTokens/full",
            new
            {
                textDocument = new { uri = documentUri },
            },
            CancellationToken.None);
        Assert.IsGreaterThan(0, tokenResponse.GetProperty("result").GetProperty("data").GetArrayLength());

        var formattingResponse = await client.SendRequestAsync(
            "textDocument/formatting",
            new
            {
                textDocument = new { uri = documentUri },
                options = new { tabSize = 4, insertSpaces = true },
            },
            CancellationToken.None);
        Assert.IsGreaterThan(0, formattingResponse.GetProperty("result").GetArrayLength());
    }

    [TestMethod]
    public async Task Protocol_publishes_diagnostics_notifications()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <Button Text= />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        await using var client = await StartClientAsync();
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        await OpenDocumentAsync(client, documentUri, text);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var diagnosticsNotification = await client.WaitForNotificationAsync(
            "textDocument/publishDiagnostics",
            cts.Token);

        var diagnostics = diagnosticsNotification.GetProperty("params").GetProperty("diagnostics");
        Assert.IsGreaterThan(0, diagnostics.GetArrayLength());
    }

    [TestMethod]
    public async Task Protocol_accepts_percent_encoded_windows_file_uris_for_provider_requests()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                State<string> SelectedId = new State<string>("todo-1");
                var current = SelectedId.Value;
                render <TodoCard AutomationProperties.Name={SelectedId.Value} />;
            }
            """);

        await using var client = await StartClientAsync();
        var documentUri = CreatePercentEncodedWindowsFileUri(tempFile.FilePath);
        await OpenDocumentAsync(client, documentUri, tempFile.Text);

        var hoverPosition = GetLineAndCharacter(
            tempFile.Text,
            tempFile.Text.IndexOf("TodoCard", StringComparison.Ordinal) + 1);
        var hoverResponse = await client.SendRequestAsync(
            "textDocument/hover",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = hoverPosition.Line, character = hoverPosition.Character },
            },
            CancellationToken.None);
        var hasHoverError = hoverResponse.TryGetProperty("error", out var hoverError);
        Assert.IsFalse(
            hasHoverError,
            hasHoverError ? hoverError.GetRawText() : hoverResponse.GetRawText());
        StringAssert.Contains(
            hoverResponse.GetProperty("result").GetProperty("contents").GetProperty("value").GetString(),
            "TodoCard");

        var tokenResponse = await client.SendRequestAsync(
            "textDocument/semanticTokens/full",
            new
            {
                textDocument = new { uri = documentUri },
            },
            CancellationToken.None);
        var hasTokenError = tokenResponse.TryGetProperty("error", out var tokenError);
        Assert.IsFalse(
            hasTokenError,
            hasTokenError ? tokenError.GetRawText() : tokenResponse.GetRawText());
        Assert.IsGreaterThan(0, tokenResponse.GetProperty("result").GetProperty("data").GetArrayLength());
    }

    private static async Task<LanguageServerClient> StartClientAsync()
    {
        var serverExecutablePath = LanguageServerTestPaths.GetServerExecutablePath();

        Assert.IsTrue(File.Exists(serverExecutablePath), $"Missing server executable at {serverExecutablePath}");

        var client = await LanguageServerClient.StartAsync(serverExecutablePath, RepoRoot);
        await client.SendRequestAsync(
            "initialize",
            new
            {
                processId = Environment.ProcessId,
                rootUri = new Uri(RepoRoot).AbsoluteUri,
                capabilities = new { },
            },
            CancellationToken.None);
        await client.SendNotificationAsync("initialized", new { }, CancellationToken.None);
        return client;
    }

    private static async Task OpenDocumentAsync(LanguageServerClient client, string documentUri, string text)
    {
        await client.SendNotificationAsync(
            "textDocument/didOpen",
            new
            {
                textDocument = new
                {
                    uri = documentUri,
                    languageId = "csxaml",
                    version = 1,
                    text,
                },
            },
            CancellationToken.None);
    }

    private static (int Line, int Character) GetLineAndCharacter(string text, int offset)
    {
        var line = 0;
        var character = 0;
        for (var index = 0; index < offset && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                character = 0;
                continue;
            }

            if (text[index] != '\r')
            {
                character++;
            }
        }

        return (line, character);
    }

    private static string CreatePercentEncodedWindowsFileUri(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        var normalizedPath = fullPath.Replace('\\', '/');
        return $"file:///{Uri.EscapeDataString(normalizedPath[..2])}/{normalizedPath[3..]}";
    }
}
