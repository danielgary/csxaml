namespace Csxaml.Runtime;

/// <summary>
/// Describes a source span that produced a runtime node or diagnostic frame.
/// </summary>
/// <param name="FilePath">The source file path that contains the span.</param>
/// <param name="StartLine">The one-based line number where the span begins.</param>
/// <param name="StartColumn">The one-based column number where the span begins.</param>
/// <param name="EndLine">The one-based line number where the span ends.</param>
/// <param name="EndColumn">The one-based column number where the span ends.</param>
/// <param name="ComponentName">The component name associated with the span, when known.</param>
/// <param name="TagName">The tag name associated with the span, when known.</param>
/// <param name="MemberName">The property, event, or injected member associated with the span, when known.</param>
public sealed record CsxamlSourceInfo(
    string FilePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string? ComponentName = null,
    string? TagName = null,
    string? MemberName = null);
