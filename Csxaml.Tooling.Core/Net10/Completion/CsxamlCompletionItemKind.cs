namespace Csxaml.Tooling.Core.Completion;

/// <summary>
/// Classifies a CSXAML completion item for editor hosts.
/// </summary>
public enum CsxamlCompletionItemKind
{
    /// <summary>
    /// A type or tag completion.
    /// </summary>
    Class,

    /// <summary>
    /// An event completion.
    /// </summary>
    Event,

    /// <summary>
    /// A language keyword completion.
    /// </summary>
    Keyword,

    /// <summary>
    /// A method completion.
    /// </summary>
    Method,

    /// <summary>
    /// A namespace completion.
    /// </summary>
    Namespace,

    /// <summary>
    /// A parameter completion.
    /// </summary>
    Parameter,

    /// <summary>
    /// A property completion.
    /// </summary>
    Property,

    /// <summary>
    /// A local variable completion.
    /// </summary>
    Variable,
}
