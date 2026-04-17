namespace Csxaml.Generator;

internal sealed class AttachedPropertyBindingResolver
{
    private readonly IReadOnlyDictionary<string, string> _aliases;
    private readonly string? _currentNamespace;
    private readonly IReadOnlyList<string> _importedNamespaces;

    public AttachedPropertyBindingResolver(ParsedComponent component)
    {
        _currentNamespace = component.File.Namespace?.NamespaceName;
        _importedNamespaces = component.File.UsingDirectives
            .Where(directive => !directive.IsStatic && directive.Alias is null)
            .Select(directive => directive.QualifiedName)
            .ToList();
        _aliases = component.File.UsingDirectives
            .Where(directive => !directive.IsStatic && directive.Alias is not null)
            .ToDictionary(
                directive => directive.Alias!,
                directive => directive.QualifiedName,
                StringComparer.Ordinal);
    }

    public AttachedPropertyMetadata ResolveOrThrow(
        SourceDocument source,
        string tagName,
        PropertyNode property)
    {
        var resolution = AttachedPropertyReferenceResolver.Resolve(
            property.OwnerName!,
            property.PropertyName,
            _currentNamespace,
            _importedNamespaces,
            _aliases);

        return resolution.Kind switch
        {
            AttachedPropertyResolutionKind.Resolved => resolution.Property!,
            AttachedPropertyResolutionKind.Ambiguous => throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                $"ambiguous attached property '{property.Name}' on '{tagName}'"),
            _ => throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                $"unknown attached property '{property.Name}' on '{tagName}'")
        };
    }
}
