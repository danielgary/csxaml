using Csxaml.ControlMetadata;
using Csxaml.Generator;

namespace Csxaml.Tooling.Core.Markup;

internal static class CsxamlAttachedPropertyResolver
{
    public static bool TryResolve(
        PropertyNode property,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string? currentNamespace,
        out AttachedPropertyMetadata? metadata)
    {
        return TryResolve(
            property.OwnerName!,
            property.PropertyName,
            usingDirectives,
            currentNamespace,
            out metadata);
    }

    public static bool TryResolve(
        string ownerName,
        string propertyName,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string? currentNamespace,
        out AttachedPropertyMetadata? metadata)
    {
        var resolution = AttachedPropertyReferenceResolver.Resolve(
            ownerName,
            propertyName,
            currentNamespace,
            usingDirectives
                .Where(directive => !directive.IsStatic && directive.Alias is null)
                .Select(directive => directive.QualifiedName),
            usingDirectives
                .Where(directive => !directive.IsStatic && directive.Alias is not null)
                .Select(directive => new KeyValuePair<string, string>(directive.Alias!, directive.QualifiedName)));

        metadata = resolution.Property;
        return resolution.Kind == AttachedPropertyResolutionKind.Resolved;
    }
}
