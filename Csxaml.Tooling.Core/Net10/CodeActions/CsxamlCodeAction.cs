namespace Csxaml.Tooling.Core.CodeActions;

/// <summary>
/// Describes an editor code action and the text edits it applies.
/// </summary>
/// <param name="Title">The user-facing title of the code action.</param>
/// <param name="Edits">The text edits applied by the action.</param>
public sealed record CsxamlCodeAction(
    string Title,
    IReadOnlyList<CsxamlCodeActionEdit> Edits);
