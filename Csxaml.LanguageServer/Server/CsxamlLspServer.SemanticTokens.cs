using System.Globalization;
using System.Text.Json;
using Csxaml.LanguageServer.Documents;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.LanguageServer.Server;

internal sealed partial class CsxamlLspServer
{
    private static readonly string[] TokenTypes =
    [
        "class",
        "event",
        "interface",
        "keyword",
        "method",
        "parameter",
        "property",
        "variable",
    ];
    private static readonly string[] TokenModifiers = ["declaration", "defaultLibrary", "readonly"];

    private object HandleSemanticTokens(JsonElement root)
    {
        var uri = root.GetProperty("params").GetProperty("textDocument").GetProperty("uri").GetString()!;
        var text = _documents.GetOrLoad(uri);
        var tokens = _semanticTokenService.GetTokens(new Uri(uri).LocalPath, text);

        var data = new List<int>();
        var previousLine = 0;
        var previousCharacter = 0;
        foreach (var token in tokens)
        {
            var position = LspTextPositionConverter.GetLineAndCharacter(text, token.Start);
            var deltaLine = position.Line - previousLine;
            var deltaCharacter = deltaLine == 0
                ? position.Character - previousCharacter
                : position.Character;
            data.Add(deltaLine);
            data.Add(deltaCharacter);
            data.Add(token.Length);
            data.Add(GetTokenTypeIndex(token.Type));
            data.Add(GetTokenModifierBits(token));
            previousLine = position.Line;
            previousCharacter = position.Character;
        }

        return new { data };
    }

    private static int GetTokenModifierBits(CsxamlSemanticToken token)
    {
        var bits = 0;
        if (token.IsDeclaration)
        {
            bits |= 1 << 0;
        }

        if (token.IsDefaultLibrary)
        {
            bits |= 1 << 1;
        }

        if (token.IsReadOnly)
        {
            bits |= 1 << 2;
        }

        return bits;
    }

    private static int GetTokenTypeIndex(CsxamlSemanticTokenType tokenType)
    {
        return tokenType switch
        {
            CsxamlSemanticTokenType.Class => 0,
            CsxamlSemanticTokenType.Event => 1,
            CsxamlSemanticTokenType.Interface => 2,
            CsxamlSemanticTokenType.Keyword => 3,
            CsxamlSemanticTokenType.Method => 4,
            CsxamlSemanticTokenType.Parameter => 5,
            CsxamlSemanticTokenType.Property => 6,
            CsxamlSemanticTokenType.Variable => 7,
            _ => 0,
        };
    }
}
