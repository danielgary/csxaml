using Csxaml.Generator;
using Csxaml.Tooling.Core.CSharp;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Diagnostics;

public sealed class CsxamlDiagnosticService
{
    private readonly CsxamlCSharpDiagnosticService _csharpDiagnosticService = new();
    private readonly Parser _parser = new();
    private readonly Validator _validator = new();

    public IReadOnlyList<CsxamlEditorDiagnostic> GetDiagnostics(string filePath, string text)
    {
        var projectFile = CsxamlProjectLocator.FindOwningProjectFile(filePath);
        if (projectFile is null)
        {
            return Array.Empty<CsxamlEditorDiagnostic>();
        }

        var project = CsxamlProjectFileReader.Read(projectFile);
        var referencedProjects = CsxamlProjectReferenceResolver.ResolveTransitive(project);
        var parsedComponents = new List<ParsedComponent>();
        foreach (var sourceFile in Directory.EnumerateFiles(project.ProjectDirectory, "*.csxaml", SearchOption.AllDirectories))
        {
            if (sourceFile.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || sourceFile.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var currentText = string.Equals(sourceFile, filePath, StringComparison.OrdinalIgnoreCase)
                ? text
                : File.ReadAllText(sourceFile);

            try
            {
                var source = new SourceDocument(sourceFile, currentText);
                parsedComponents.Add(new ParsedComponent(source, _parser.Parse(source)));
            }
            catch (DiagnosticException exception) when (string.Equals(sourceFile, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return new[] { FromDiagnostic(exception.Diagnostic) };
            }
        }

        try
        {
            var references = CsxamlProjectOutputResolver.ResolveAssemblyClosurePaths(referencedProjects)
                .Concat(CsxamlProjectOutputResolver.ResolveAssemblyClosurePaths(
                    new[] { project },
                    includePrimaryAssemblies: false))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var projectContext = new ProjectGenerationContext(
                project.AssemblyName,
                project.DefaultNamespace,
                $"{project.DefaultNamespace}.__CsxamlGenerated");
            _validator.Validate(
                parsedComponents,
                projectContext,
                references,
                ignoreReferencedComponentLoadFailures: true);
            return _csharpDiagnosticService.GetDiagnostics(filePath, text);
        }
        catch (DiagnosticException exception)
        {
            return string.Equals(exception.Diagnostic.FilePath, filePath, StringComparison.OrdinalIgnoreCase)
                ? new[] { FromDiagnostic(exception.Diagnostic) }
                : _csharpDiagnosticService.GetDiagnostics(filePath, text);
        }
    }

    private static CsxamlEditorDiagnostic FromDiagnostic(Diagnostic diagnostic)
    {
        var startLine = Math.Max(diagnostic.Line - 1, 0);
        var startCharacter = Math.Max(diagnostic.Column - 1, 0);
        var endLine = Math.Max(diagnostic.EndLine - 1, startLine);
        var endCharacter = Math.Max(
            diagnostic.EndColumn - 1,
            endLine == startLine ? startCharacter + 1 : 0);
        return new CsxamlEditorDiagnostic(
            startLine,
            startCharacter,
            endLine,
            endCharacter,
            diagnostic.Message);
    }
}
