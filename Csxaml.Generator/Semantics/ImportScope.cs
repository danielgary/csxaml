namespace Csxaml.Generator;

internal sealed class ImportScope
{
    private readonly IReadOnlyDictionary<string, string> _aliases;

    private ImportScope(
        IReadOnlyList<string> importedNamespaces,
        IReadOnlyDictionary<string, string> aliases)
    {
        ImportedNamespaces = importedNamespaces;
        _aliases = aliases;
    }

    public IReadOnlyList<string> ImportedNamespaces { get; }

    public static ImportScope Create(SourceDocument source, IReadOnlyList<UsingDirectiveDefinition> directives)
    {
        var importedNamespaces = new List<string>();
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var directive in directives)
        {
            if (directive.IsStatic)
            {
                continue;
            }

            if (directive.Alias is null)
            {
                importedNamespaces.Add(directive.QualifiedName);
                continue;
            }

            if (!aliases.TryAdd(directive.Alias, directive.QualifiedName))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    directive.Span,
                    $"duplicate using alias '{directive.Alias}'");
            }
        }

        return new ImportScope(importedNamespaces, aliases);
    }

    public bool TryGetAliasNamespace(string alias, out string? namespaceName)
    {
        return _aliases.TryGetValue(alias, out namespaceName);
    }
}
