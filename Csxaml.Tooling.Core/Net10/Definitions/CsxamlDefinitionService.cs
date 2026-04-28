using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.CSharp;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Definitions;

/// <summary>
/// Provides go-to-definition support for CSXAML markup and embedded C# expressions.
/// </summary>
public sealed class CsxamlDefinitionService
{
    private readonly CsxamlCSharpDefinitionService _csharpDefinitionService = new();
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    /// <summary>
    /// Gets the definition location for the symbol at a source position.
    /// </summary>
    /// <param name="filePath">The CSXAML file path.</param>
    /// <param name="text">The current CSXAML source text.</param>
    /// <param name="position">The zero-based source offset to resolve.</param>
    /// <returns>The definition location, or <see langword="null"/> when no definition is available.</returns>
    public CsxamlDefinitionLocation? GetDefinition(string filePath, string text, int position)
    {
        var workspace = _workspaceLoader.Load(filePath, text);
        var scan = CsxamlMarkupScanner.Scan(text);
        var element = scan.Elements.FirstOrDefault(
            candidate => position >= candidate.NameStart && position <= candidate.NameStart + candidate.NameLength);
        if (element is not null)
        {
            var currentNamespace = scan.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;
            var resolvedTag = _tagResolver.Resolve(element.TagName, scan.UsingDirectives, currentNamespace, workspace);
            if (resolvedTag.Component is not null)
            {
                return new CsxamlDefinitionLocation(
                    resolvedTag.Component.FilePath,
                    resolvedTag.Component.NameStart,
                    resolvedTag.Component.NameLength);
            }
        }

        var csharpDefinition = _csharpDefinitionService.GetDefinition(filePath, text, position);
        return csharpDefinition is null
            ? null
            : new CsxamlDefinitionLocation(
                csharpDefinition.Value.FilePath,
                csharpDefinition.Value.Start,
                csharpDefinition.Value.Length);
    }
}
