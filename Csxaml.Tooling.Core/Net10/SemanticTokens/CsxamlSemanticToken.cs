namespace Csxaml.Tooling.Core.SemanticTokens;

public sealed record CsxamlSemanticToken(
    int Start,
    int Length,
    CsxamlSemanticTokenType Type,
    bool IsDeclaration = false,
    bool IsDefaultLibrary = false,
    bool IsReadOnly = false);
