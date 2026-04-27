namespace Csxaml.Tooling.Core.Completion;

/// <summary>
/// Describes the result of resolving a CSXAML tag.
/// </summary>
public enum CsxamlResolvedTagKind
{
    /// <summary>
    /// The tag did not resolve to a known symbol.
    /// </summary>
    None,

    /// <summary>
    /// The tag resolved to a built-in native control.
    /// </summary>
    Native,

    /// <summary>
    /// The tag resolved to an external control discovered from project references.
    /// </summary>
    External,

    /// <summary>
    /// The tag resolved to a CSXAML component.
    /// </summary>
    Component,

    /// <summary>
    /// The tag matched more than one possible symbol.
    /// </summary>
    Ambiguous,
}
