namespace Csxaml.Tooling.Core.SemanticTokens;

/// <summary>
/// Classifies a CSXAML semantic token for editor hosts.
/// </summary>
public enum CsxamlSemanticTokenType
{
    /// <summary>
    /// A class, component, or control type token.
    /// </summary>
    Class,

    /// <summary>
    /// An event token.
    /// </summary>
    Event,

    /// <summary>
    /// An interface token.
    /// </summary>
    Interface,

    /// <summary>
    /// A language keyword token.
    /// </summary>
    Keyword,

    /// <summary>
    /// A method token.
    /// </summary>
    Method,

    /// <summary>
    /// A parameter token.
    /// </summary>
    Parameter,

    /// <summary>
    /// A property token.
    /// </summary>
    Property,

    /// <summary>
    /// A local variable token.
    /// </summary>
    Variable,
}
