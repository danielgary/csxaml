namespace Csxaml.Tooling.Core.SemanticTokens;

/// <summary>
/// Describes a semantic token produced for CSXAML editor coloring.
/// </summary>
/// <param name="Start">The zero-based start offset of the token.</param>
/// <param name="Length">The token length.</param>
/// <param name="Type">The semantic token type.</param>
/// <param name="IsDeclaration">A value indicating whether the token declares a symbol.</param>
/// <param name="IsDefaultLibrary">A value indicating whether the token belongs to the built-in CSXAML or WinUI surface.</param>
/// <param name="IsReadOnly">A value indicating whether the token represents a read-only construct.</param>
public sealed record CsxamlSemanticToken(
    int Start,
    int Length,
    CsxamlSemanticTokenType Type,
    bool IsDeclaration = false,
    bool IsDefaultLibrary = false,
    bool IsReadOnly = false);
