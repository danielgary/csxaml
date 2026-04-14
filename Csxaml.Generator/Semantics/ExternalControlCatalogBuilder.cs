namespace Csxaml.Generator;

internal sealed class ExternalControlCatalogBuilder
{
    private readonly ExternalControlMetadataBuilder _metadataBuilder = new();

    public NativeControlCatalog Build(
        IReadOnlyList<ParsedComponent> components,
        IReadOnlyList<string> referencePaths)
    {
        if (referencePaths.Count == 0)
        {
            return new NativeControlCatalog(Array.Empty<ControlMetadataModel>());
        }

        var candidates = ExternalControlCandidateCollector.Collect(components);
        var controls = new List<ControlMetadataModel>();
        var unsupported = new Dictionary<string, string>(StringComparer.Ordinal);

        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);
        foreach (var clrTypeName in candidates)
        {
            if (TryGetBuiltIn(clrTypeName, out _))
            {
                continue;
            }

            var matches = resolver.FindTypes(clrTypeName);
            if (matches.Count == 0)
            {
                continue;
            }

            if (matches.Count > 1)
            {
                unsupported[clrTypeName] = "multiple referenced assemblies declare the same control type";
                continue;
            }

            var controlType = matches[0];
            if (_metadataBuilder.TryBuild(controlType, out var metadata, out var reason))
            {
                controls.Add(metadata!);
                continue;
            }

            unsupported[clrTypeName] = reason ?? "unsupported control shape";
        }

        return new NativeControlCatalog(controls, unsupported);
    }

    private static bool TryGetBuiltIn(string clrTypeName, out ControlMetadataModel? control)
    {
        control = ControlMetadataRegistry.Controls.SingleOrDefault(
            candidate => string.Equals(candidate.ClrTypeName, clrTypeName, StringComparison.Ordinal));
        return control is not null;
    }
}
