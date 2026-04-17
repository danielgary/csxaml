namespace Csxaml.ControlMetadata;

public static class AttachedPropertyReferenceResolver
{
    public static AttachedPropertyResolutionResult Resolve(
        string ownerReference,
        string propertyName,
        string? currentNamespace,
        IEnumerable<string> importedNamespaces,
        IEnumerable<KeyValuePair<string, string>> aliases)
    {
        var importedNamespaceList = importedNamespaces as IReadOnlyList<string> ??
            importedNamespaces.ToArray();
        var aliasMap = aliases as IReadOnlyDictionary<string, string> ??
            aliases.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var candidates = AttachedPropertyMetadataRegistry.GetPropertiesByName(propertyName);

        AttachedPropertyMetadata? match = null;
        foreach (var candidate in candidates)
        {
            if (!MatchesOwnerReference(candidate, ownerReference, currentNamespace, importedNamespaceList, aliasMap))
            {
                continue;
            }

            if (match is null)
            {
                match = candidate;
                continue;
            }

            if (!string.Equals(match.QualifiedName, candidate.QualifiedName, StringComparison.Ordinal))
            {
                return new AttachedPropertyResolutionResult(
                    AttachedPropertyResolutionKind.Ambiguous,
                    null);
            }
        }

        return match is not null
            ? new AttachedPropertyResolutionResult(AttachedPropertyResolutionKind.Resolved, match)
            : new AttachedPropertyResolutionResult(AttachedPropertyResolutionKind.Unknown, null);
    }

    private static bool MatchesOwnerReference(
        AttachedPropertyMetadata property,
        string ownerReference,
        string? currentNamespace,
        IReadOnlyList<string> importedNamespaces,
        IReadOnlyDictionary<string, string> aliases)
    {
        if (aliases.TryGetValue(ownerReference, out var aliasTarget))
        {
            return string.Equals(aliasTarget, property.ClrOwnerTypeName, StringComparison.Ordinal);
        }

        if (!MatchesSimpleOwnerName(property, ownerReference))
        {
            return false;
        }

        return IsVisibleNamespace(property.ClrOwnerTypeName, currentNamespace, importedNamespaces);
    }

    private static bool MatchesSimpleOwnerName(
        AttachedPropertyMetadata property,
        string ownerReference)
    {
        return string.Equals(ownerReference, property.OwnerName, StringComparison.Ordinal) ||
            GetTypeName(property.ClrOwnerTypeName).Equals(ownerReference, StringComparison.Ordinal);
    }

    private static bool IsVisibleNamespace(
        string clrOwnerTypeName,
        string? currentNamespace,
        IReadOnlyList<string> importedNamespaces)
    {
        var ownerNamespace = GetNamespace(clrOwnerTypeName);
        if (currentNamespace is not null && ownerNamespace.Equals(currentNamespace, StringComparison.Ordinal))
        {
            return true;
        }

        for (var i = 0; i < importedNamespaces.Count; i++)
        {
            if (ownerNamespace.Equals(importedNamespaces[i], StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static ReadOnlySpan<char> GetNamespace(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0 ? ReadOnlySpan<char>.Empty : clrTypeName.AsSpan(0, separatorIndex);
    }

    private static ReadOnlySpan<char> GetTypeName(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0
            ? clrTypeName.AsSpan()
            : clrTypeName.AsSpan(separatorIndex + 1);
    }
}
