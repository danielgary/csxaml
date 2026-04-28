using Csxaml.ControlMetadata;
using Csxaml.Generator;
using Csxaml.Tooling.Core.Markup;

namespace Csxaml.Tooling.Core.Projects;

internal sealed class CsxamlWorkspaceComponentCache
{
    private readonly Dictionary<string, CsxamlCachedComponentSymbol> _symbolsByFilePath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Parser _parser;

    public CsxamlWorkspaceComponentCache(Parser parser)
    {
        _parser = parser;
    }

    public IReadOnlyList<CsxamlWorkspaceComponentSymbol> GetProjectComponents(
        CsxamlProjectInfo project,
        string? currentFilePath,
        string? currentText)
    {
        var symbols = new List<CsxamlWorkspaceComponentSymbol>();
        foreach (var filePath in Directory.EnumerateFiles(project.ProjectDirectory, "*.csxaml", SearchOption.AllDirectories))
        {
            if (IsIgnoredPath(filePath))
            {
                continue;
            }

            if (string.Equals(filePath, currentFilePath, StringComparison.OrdinalIgnoreCase) &&
                currentText is not null)
            {
                if (TryParseComponent(project, filePath, currentText, out var currentSymbol))
                {
                    symbols.Add(currentSymbol!);
                }

                continue;
            }

            if (TryGetCachedSymbol(project, filePath, out var cachedSymbol))
            {
                symbols.Add(cachedSymbol!);
            }
        }

        return symbols;
    }

    private bool TryGetCachedSymbol(
        CsxamlProjectInfo project,
        string filePath,
        out CsxamlWorkspaceComponentSymbol? symbol)
    {
        var stamp = CsxamlFileStamp.Read(filePath);
        if (_symbolsByFilePath.TryGetValue(filePath, out var cached) &&
            cached.Stamp == stamp)
        {
            symbol = cached.Symbol;
            return symbol is not null;
        }

        var text = File.ReadAllText(filePath);
        var parsed = TryParseComponent(project, filePath, text, out symbol);
        _symbolsByFilePath[filePath] = new CsxamlCachedComponentSymbol(stamp, symbol);
        return parsed;
    }

    private bool TryParseComponent(
        CsxamlProjectInfo project,
        string filePath,
        string text,
        out CsxamlWorkspaceComponentSymbol? symbol)
    {
        try
        {
            var source = new SourceDocument(filePath, text);
            var file = _parser.Parse(source);
            var definition = file.Component;
            symbol = CreateComponentSymbol(
                project,
                filePath,
                file.Namespace?.NamespaceName ?? project.DefaultNamespace,
                definition.Name,
                definition.Parameters
                    .Select(parameter => new ComponentParameterMetadata(parameter.Name, parameter.TypeName))
                    .ToList(),
                definition.SupportsDefaultSlot,
                definition.Span.Start,
                definition.Name.Length);
            return true;
        }
        catch (DiagnosticException)
        {
            var declaration = CsxamlComponentSignatureScanner.Scan(text).FirstOrDefault();
            if (declaration is null)
            {
                symbol = null;
                return false;
            }

            symbol = CreateComponentSymbol(
                project,
                filePath,
                CsxamlNamespaceDirectiveScanner.Scan(text)?.NamespaceName ?? project.DefaultNamespace,
                declaration.Name,
                declaration.Parameters
                    .Select(parameter => new ComponentParameterMetadata(parameter.Name, parameter.TypeName))
                    .ToList(),
                supportsDefaultSlot: false,
                declaration.NameStart,
                declaration.NameLength);
            return true;
        }
    }

    private static CsxamlWorkspaceComponentSymbol CreateComponentSymbol(
        CsxamlProjectInfo project,
        string filePath,
        string namespaceName,
        string name,
        IReadOnlyList<ComponentParameterMetadata> parameters,
        bool supportsDefaultSlot,
        int nameStart,
        int nameLength)
    {
        var metadata = new ComponentMetadata(
            name,
            namespaceName,
            project.AssemblyName,
            $"{namespaceName}.{name}Component",
            parameters.Count == 0 ? null : $"{namespaceName}.{name}Props",
            parameters,
            supportsDefaultSlot);
        return new CsxamlWorkspaceComponentSymbol(metadata, filePath, nameStart, nameLength);
    }

    private static bool IsIgnoredPath(string filePath)
    {
        return filePath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || filePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }
}
