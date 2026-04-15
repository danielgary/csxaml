namespace Csxaml.Tooling.Core.Diagnostics;

public sealed record CsxamlEditorDiagnostic(
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter,
    string Message);
