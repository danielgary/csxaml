namespace Csxaml.Tooling.Core.CodeActions;

/// <summary>
/// Describes a line-and-character text edit produced by a CSXAML code action.
/// </summary>
/// <param name="StartLine">The zero-based start line of the edit.</param>
/// <param name="StartCharacter">The zero-based start character of the edit.</param>
/// <param name="EndLine">The zero-based end line of the edit.</param>
/// <param name="EndCharacter">The zero-based end character of the edit.</param>
/// <param name="NewText">The replacement text.</param>
public sealed record CsxamlCodeActionEdit(
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter,
    string NewText);
