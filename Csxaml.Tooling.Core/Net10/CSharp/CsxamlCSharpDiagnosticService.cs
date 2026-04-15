using Microsoft.CodeAnalysis;
using Csxaml.Tooling.Core.Diagnostics;
using Csxaml.Tooling.Core.Text;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpDiagnosticService
{
    private readonly CsxamlCSharpCompilationFactory _compilationFactory = new();
    private readonly CsxamlCSharpProjectionBuilder _projectionBuilder = new();

    public IReadOnlyList<CsxamlEditorDiagnostic> GetDiagnostics(string filePath, string text)
    {
        var projection = _projectionBuilder.Build(filePath, text);
        if (projection is null)
        {
            return Array.Empty<CsxamlEditorDiagnostic>();
        }

        var compilation = _compilationFactory.Create(filePath, projection);
        return compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => MapDiagnostic(text, projection, diagnostic))
            .Where(diagnostic => diagnostic is not null)
            .Cast<CsxamlEditorDiagnostic>()
            .Distinct()
            .ToList();
    }

    private static CsxamlEditorDiagnostic? MapDiagnostic(
        string originalText,
        CsxamlProjectedDocument projection,
        Diagnostic diagnostic)
    {
        var span = diagnostic.Location.SourceSpan;
        if (!projection.TryMapProjectedRange(span.Start, span.End, out var originalStart, out var originalEnd))
        {
            return null;
        }

        var start = CsxamlTextCoordinateConverter.GetLineAndCharacter(originalText, originalStart);
        var end = CsxamlTextCoordinateConverter.GetLineAndCharacter(originalText, originalEnd);
        return new CsxamlEditorDiagnostic(
            start.Line,
            start.Character,
            end.Line,
            end.Character,
            diagnostic.GetMessage());
    }
}
