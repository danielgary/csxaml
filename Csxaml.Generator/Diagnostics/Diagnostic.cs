namespace Csxaml.Generator;

internal sealed record Diagnostic(
    string FilePath,
    int Line,
    int Column,
    int EndLine,
    int EndColumn,
    string Message)
{
    public override string ToString()
    {
        return $"{FilePath}({Line},{Column}): {Message}";
    }
}
