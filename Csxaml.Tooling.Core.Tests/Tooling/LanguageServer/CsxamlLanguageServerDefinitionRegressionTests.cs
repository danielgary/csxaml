using System.Text.Json;

namespace Csxaml.Tooling.Core.Tests.Tooling.LanguageServer;

[TestClass]
public sealed class CsxamlLanguageServerDefinitionRegressionTests
{
    private static readonly string RepoRoot = LanguageServerTestPaths.RepoRoot;

    [TestMethod]
    public async Task Protocol_resolves_framework_state_definitions_in_component_helper_code()
    {
        var filePath = Path.Combine(RepoRoot, "Csxaml.Demo", "Components", "TodoBoard.csxaml");
        var text = File.ReadAllText(filePath);
        var documentUri = new Uri(filePath).AbsoluteUri;
        var stateOffset = text.IndexOf("State<List<TodoItemModel>>", StringComparison.Ordinal) + 1;
        var position = GetLineAndCharacter(text, stateOffset);

        await using var client = await StartClientAsync();
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

        var response = await client.SendRequestAsync(
            "textDocument/definition",
            new
            {
                textDocument = new { uri = documentUri },
                position = new { line = position.Line, character = position.Character },
            },
            CancellationToken.None);

        var result = response.GetProperty("result")[0];
        StringAssert.EndsWith(
            result.GetProperty("uri").GetString(),
            "/Csxaml.Runtime/State/State.cs");
    }

    [TestMethod]
    public async Task Protocol_serves_helper_return_type_semantic_tokens_for_todo_board()
    {
        var filePath = Path.Combine(RepoRoot, "Csxaml.Demo", "Components", "TodoBoard.csxaml");
        var text = File.ReadAllText(filePath);
        var documentUri = new Uri(filePath).AbsoluteUri;
        var returnTypeOffset = text.IndexOf("TodoItemModel SelectedItem", StringComparison.Ordinal);
        var returnTypePosition = GetLineAndCharacter(text, returnTypeOffset);

        await using var client = await StartClientAsync();
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

        var response = await client.SendRequestAsync(
            "textDocument/semanticTokens/full",
            new
            {
                textDocument = new { uri = documentUri },
            },
            CancellationToken.None);
        var tokens = DecodeTokens(response.GetProperty("result").GetProperty("data"));

        Assert.IsTrue(
            tokens.Any(token =>
                token.Line == returnTypePosition.Line &&
                token.Character == returnTypePosition.Character &&
                token.Length == "TodoItemModel".Length &&
                token.Type == "class"),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Line}:{token.Character}:{token.Length}")));
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

    private static IReadOnlyList<DecodedSemanticToken> DecodeTokens(JsonElement data)
    {
        const string classTokenType = "class";
        const string eventTokenType = "event";
        const string interfaceTokenType = "interface";
        const string keywordTokenType = "keyword";
        const string methodTokenType = "method";
        const string parameterTokenType = "parameter";
        const string propertyTokenType = "property";
        const string variableTokenType = "variable";
        var tokenTypes = new[]
        {
            classTokenType,
            eventTokenType,
            interfaceTokenType,
            keywordTokenType,
            methodTokenType,
            parameterTokenType,
            propertyTokenType,
            variableTokenType,
        };

        var values = data.EnumerateArray().Select(element => element.GetInt32()).ToArray();
        var tokens = new List<DecodedSemanticToken>(values.Length / 5);
        var line = 0;
        var character = 0;

        for (var index = 0; index < values.Length; index += 5)
        {
            var deltaLine = values[index];
            var deltaCharacter = values[index + 1];
            var length = values[index + 2];
            var type = tokenTypes[values[index + 3]];

            line += deltaLine;
            character = deltaLine == 0
                ? character + deltaCharacter
                : deltaCharacter;

            tokens.Add(new DecodedSemanticToken(line, character, length, type));
        }

        return tokens;
    }

    private sealed record DecodedSemanticToken(int Line, int Character, int Length, string Type);
}
