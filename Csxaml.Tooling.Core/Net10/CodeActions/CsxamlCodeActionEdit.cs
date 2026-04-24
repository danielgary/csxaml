namespace Csxaml.Tooling.Core.CodeActions;

public sealed record CsxamlCodeActionEdit(
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter,
    string NewText);
