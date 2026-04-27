namespace Csxaml.ControlMetadata;

/// <summary>
/// Resolves attached-property owner references against generated metadata and namespace imports.
/// </summary>
public static class AttachedPropertyReferenceResolver
{
    /// <summary>
    /// Resolves an attached-property reference from markup context.
    /// </summary>
    /// <param name="ownerReference">The owner reference written before the property name.</param>
    /// <param name="propertyName">The unqualified attached property name.</param>
    /// <param name="currentNamespace">The current component namespace, or <see langword="null"/> when none is available.</param>
    /// <param name="importedNamespaces">The namespaces imported into the current markup scope.</param>
    /// <param name="aliases">Alias mappings from markup owner names to fully qualified CLR type names.</param>
    /// <returns>The resolution result describing the matched property or failure reason.</returns>
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
