namespace Csxaml.Generator;

internal sealed record ResolvedTag(
    ResolvedTagKind Kind,
    string RuntimeTagName,
    ControlMetadataModel? NativeControl,
    ComponentCatalogEntry? Component);
