namespace Csxaml.Generator;

internal sealed class DiagnosticException : Exception
{
    public DiagnosticException(Diagnostic diagnostic)
        : base(diagnostic.ToString())
    {
        Diagnostic = diagnostic;
    }

    public Diagnostic Diagnostic { get; }
}
