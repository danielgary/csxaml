namespace Csxaml.Runtime;

public sealed record CsxamlSourceInfo(
    string FilePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string? ComponentName = null,
    string? TagName = null,
    string? MemberName = null);
