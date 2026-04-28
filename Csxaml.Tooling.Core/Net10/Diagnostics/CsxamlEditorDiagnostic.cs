namespace Csxaml.Tooling.Core.Diagnostics;

/// <summary>
/// Describes a diagnostic range and message for editor hosts.
/// </summary>
/// <param name="StartLine">The zero-based start line of the diagnostic.</param>
/// <param name="StartCharacter">The zero-based start character of the diagnostic.</param>
/// <param name="EndLine">The zero-based end line of the diagnostic.</param>
/// <param name="EndCharacter">The zero-based end character of the diagnostic.</param>
/// <param name="Message">The diagnostic message.</param>
public sealed record CsxamlEditorDiagnostic(
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter,
    string Message);
