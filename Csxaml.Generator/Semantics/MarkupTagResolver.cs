namespace Csxaml.Generator;

internal sealed class MarkupTagResolver
{
    public ResolvedTag Resolve(
        SourceDocument source,
        ParsedComponent component,
        MarkupNode node,
        CompilationContext compilation)
    {
        var imports = ImportScope.Create(source, component.File.UsingDirectives);
        var currentNamespace = component.File.Namespace?.NamespaceName ??
            compilation.Project.DefaultComponentNamespace;
        return node.Tag.Prefix is null
            ? ResolveSimpleTag(source, node, currentNamespace, imports, compilation)
            : ResolveQualifiedTag(source, node, imports, compilation);
    }

    private static ResolvedTag ResolveQualifiedTag(
        SourceDocument source,
        MarkupNode node,
        ImportScope imports,
        CompilationContext compilation)
    {
        var prefix = node.Tag.Prefix ?? throw new InvalidOperationException(
            "Qualified tag resolution requires a prefix.");
        if (!imports.TryGetAliasNamespace(prefix, out var namespaceName))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Tag.Span,
                $"unknown import alias '{prefix}' for tag '{node.TagName}'");
        }

        var componentMatches = compilation.Components.FindByNamespaceAndName(
            namespaceName!,
            node.Tag.LocalName);
        var nativeMatch = TryResolveNativeControl($"{namespaceName}.{node.Tag.LocalName}", compilation);
        var unsupportedReason = TryGetUnsupportedReason(
            $"{namespaceName}.{node.Tag.LocalName}",
            compilation);

        return ResolveImportedMatches(
            source,
            node,
            componentMatches,
            nativeMatch,
            unsupportedReason);
    }

    private static ResolvedTag ResolveSimpleTag(
        SourceDocument source,
        MarkupNode node,
        string currentNamespace,
        ImportScope imports,
        CompilationContext compilation)
    {
        if (compilation.NativeControls.TryGetBuiltIn(node.TagName, out var builtIn))
        {
            return new ResolvedTag(ResolvedTagKind.Native, builtIn!.TagName, builtIn, null);
        }

        var componentMatches = CollectComponentMatches(node, currentNamespace, imports, compilation);
        var nativeMatches = CollectImportedNativeMatches(node, imports, compilation);

        if (componentMatches.Count == 1 && nativeMatches.Count == 0)
        {
            var component = componentMatches[0];
            return new ResolvedTag(
                ResolvedTagKind.Component,
                component.ComponentTypeName,
                null,
                component);
        }

        if (componentMatches.Count > 1 || (componentMatches.Count > 0 && nativeMatches.Count > 0))
        {
            throw CreateAmbiguousImportedTag(source, node);
        }

        if (nativeMatches.Count == 1)
        {
            var native = nativeMatches[0];
            return new ResolvedTag(ResolvedTagKind.Native, native.TagName, native, null);
        }

        if (nativeMatches.Count > 1)
        {
            throw CreateAmbiguousImportedTag(source, node);
        }

        var unsupportedMatches = CollectUnsupportedImportedNativeMatches(node, imports, compilation);
        if (unsupportedMatches.Count == 1)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Tag.Span,
                $"imported control '{node.TagName}' is not supported: {unsupportedMatches[0]}");
        }

        if (unsupportedMatches.Count > 1)
        {
            throw CreateAmbiguousImportedTag(source, node);
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Tag.Span,
            $"unsupported tag name '{node.TagName}'");
    }

    private static IReadOnlyList<ComponentCatalogEntry> CollectComponentMatches(
        MarkupNode node,
        string currentNamespace,
        ImportScope imports,
        CompilationContext compilation)
    {
        var matches = new List<ComponentCatalogEntry>();
        matches.AddRange(compilation.Components.FindByNamespaceAndName(currentNamespace, node.Tag.LocalName));
        foreach (var namespaceName in imports.ImportedNamespaces)
        {
            matches.AddRange(compilation.Components.FindByNamespaceAndName(namespaceName, node.Tag.LocalName));
        }

        return matches
            .GroupBy(
                entry => $"{entry.AssemblyName}|{entry.NamespaceName}|{entry.Name}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
    }

    private static IReadOnlyList<ControlMetadataModel> CollectImportedNativeMatches(
        MarkupNode node,
        ImportScope imports,
        CompilationContext compilation)
    {
        return imports.ImportedNamespaces
            .Select(namespaceName => TryResolveNativeControl($"{namespaceName}.{node.Tag.LocalName}", compilation))
            .Where(control => control is not null)
            .Cast<ControlMetadataModel>()
            .GroupBy(control => control.ClrTypeName, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
    }

    private static IReadOnlyList<string> CollectUnsupportedImportedNativeMatches(
        MarkupNode node,
        ImportScope imports,
        CompilationContext compilation)
    {
        return imports.ImportedNamespaces
            .Select(namespaceName => TryGetUnsupportedReason($"{namespaceName}.{node.Tag.LocalName}", compilation))
            .Where(reason => reason is not null)
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static Exception CreateAmbiguousImportedTag(SourceDocument source, MarkupNode node)
    {
        return DiagnosticFactory.FromSpan(
            source,
            node.Tag.Span,
            $"ambiguous imported tag '{node.TagName}'; use a more explicit namespace import or alias");
    }

    private static ResolvedTag ResolveImportedMatches(
        SourceDocument source,
        MarkupNode node,
        IReadOnlyList<ComponentCatalogEntry> componentMatches,
        ControlMetadataModel? nativeMatch,
        string? unsupportedReason)
    {
        if (componentMatches.Count == 1 && nativeMatch is null)
        {
            var component = componentMatches[0];
            return new ResolvedTag(
                ResolvedTagKind.Component,
                component.ComponentTypeName,
                null,
                component);
        }

        if (componentMatches.Count > 1 || (componentMatches.Count > 0 && nativeMatch is not null))
        {
            throw CreateAmbiguousImportedTag(source, node);
        }

        if (nativeMatch is not null)
        {
            return new ResolvedTag(ResolvedTagKind.Native, nativeMatch.TagName, nativeMatch, null);
        }

        if (unsupportedReason is not null)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Tag.Span,
                $"imported control '{node.TagName}' is not supported: {unsupportedReason}");
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Tag.Span,
            $"unsupported tag name '{node.TagName}'");
    }

    private static ControlMetadataModel? TryResolveNativeControl(
        string clrTypeName,
        CompilationContext compilation)
    {
        if (compilation.NativeControls.TryGetBuiltInByClrTypeName(clrTypeName, out var builtIn))
        {
            return builtIn;
        }

        if (compilation.NativeControls.TryGetExternalByClrTypeName(clrTypeName, out var external))
        {
            return external;
        }

        return null;
    }

    private static string? TryGetUnsupportedReason(
        string clrTypeName,
        CompilationContext compilation)
    {
        return compilation.NativeControls.TryGetUnsupportedReason(clrTypeName, out var reason)
            ? reason
            : null;
    }
}
