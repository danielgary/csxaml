namespace Csxaml.Tooling.Core.CodeActions;

public sealed record CsxamlCodeAction(
    string Title,
    IReadOnlyList<CsxamlCodeActionEdit> Edits);
