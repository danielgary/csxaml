using System.Text.Json;

namespace Csxaml.Tooling.Core.Tests.Tooling.LanguageServer;

[TestClass]
public sealed class CsxamlLanguageServerHoverAndCodeActionTests
{
    private static readonly string RepoRoot = LanguageServerTestPaths.RepoRoot;

    [TestMethod]
    public async Task Protocol_serves_hover_for_component_tags()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <TodoCard />;
            }
            """);
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        var hoverPosition = GetLineAndCharacter(
            tempFile.Text,
            tempFile.Text.IndexOf("TodoCard", StringComparison.Ordinal) + 1);

        await using var client = await StartClientAsync();
        await OpenDocumentAsync(client, documentUri, tempFile.Text);

        var response = await client.SendRequestAsync(
            "textDocument/hover",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = hoverPosition.Line, character = hoverPosition.Character },
            },
            CancellationToken.None);

        var value = response.GetProperty("result").GetProperty("contents").GetProperty("value").GetString();
        StringAssert.Contains(value, "Component tag");
        StringAssert.Contains(value, "Csxaml.Samples.TodoApp.TodoCard");
    }

    [TestMethod]
    public async Task Protocol_serves_suggestion_based_quick_fixes()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe() {
                render <StakPanel />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        var rangeStart = GetLineAndCharacter(text, text.IndexOf("StakPanel", StringComparison.Ordinal));
        var rangeEnd = GetLineAndCharacter(text, text.IndexOf("StakPanel", StringComparison.Ordinal) + "StakPanel".Length);

        await using var client = await StartClientAsync();
        await OpenDocumentAsync(client, documentUri, text);

        var response = await client.SendRequestAsync(
            "textDocument/codeAction",
            new
            {
                textDocument = new { uri = documentUri },
                range = new
                {
                    start = new { line = rangeStart.Line, character = rangeStart.Character },
                    end = new { line = rangeEnd.Line, character = rangeEnd.Character },
                },
                context = new { diagnostics = Array.Empty<object>() },
            },
            CancellationToken.None);

        var action = response.GetProperty("result")[0];
        Assert.AreEqual("Replace 'StakPanel' with 'StackPanel'", action.GetProperty("title").GetString());
        Assert.AreEqual(
            "StackPanel",
            action.GetProperty("edit").GetProperty("changes").GetProperty(documentUri)[0].GetProperty("newText").GetString());
    }

    [TestMethod]
    public async Task Protocol_serves_hover_for_helper_methods()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                int NextCount(int value)
                {
                    return value + 1;
                }

                var next = NextCount(3);
                render <TextBlock Text={next.ToString()} />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        var hoverPosition = GetLineAndCharacter(
            text,
            text.LastIndexOf("NextCount", StringComparison.Ordinal) + 1);

        await using var client = await StartClientAsync();
        await OpenDocumentAsync(client, documentUri, text);

        var response = await client.SendRequestAsync(
            "textDocument/hover",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = hoverPosition.Line, character = hoverPosition.Character },
            },
            CancellationToken.None);

        var value = response.GetProperty("result").GetProperty("contents").GetProperty("value").GetString();
        StringAssert.Contains(value, "C# method");
        StringAssert.Contains(value, "int NextCount(int value)");
    }

    [TestMethod]
    public async Task Protocol_returns_explicit_null_hover_results_for_keywords_without_hover_content()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <TextBlock Text="Hello" />;
            }
            """;

        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);
        var documentUri = new Uri(tempFile.FilePath).AbsoluteUri;
        var hoverPosition = GetLineAndCharacter(
            text,
            text.IndexOf("render", StringComparison.Ordinal) + 1);

        await using var client = await StartClientAsync();
        await OpenDocumentAsync(client, documentUri, text);

        var response = await client.SendRequestAsync(
            "textDocument/hover",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = hoverPosition.Line, character = hoverPosition.Character },
            },
            CancellationToken.None);

        Assert.IsTrue(response.TryGetProperty("result", out var result), response.GetRawText());
        Assert.AreEqual(JsonValueKind.Null, result.ValueKind, response.GetRawText());
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

    private static Task OpenDocumentAsync(LanguageServerClient client, string documentUri, string text)
    {
        return client.SendNotificationAsync(
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
}
