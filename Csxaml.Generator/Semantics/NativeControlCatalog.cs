namespace Csxaml.Generator;

internal sealed class NativeControlCatalog
{
    private readonly IReadOnlyDictionary<string, ControlMetadataModel> _builtInsByClrTypeName;
    private readonly IReadOnlyDictionary<string, ControlMetadataModel> _externalByClrTypeName;
    private readonly IReadOnlyDictionary<string, string> _unsupportedByClrTypeName;

    public NativeControlCatalog(
        IReadOnlyList<ControlMetadataModel> externalControls,
        IReadOnlyDictionary<string, string>? unsupportedByClrTypeName = null)
    {
        _builtInsByClrTypeName = ControlMetadataRegistry.Controls.ToDictionary(
            control => control.ClrTypeName,
            StringComparer.Ordinal);
        _externalByClrTypeName = externalControls.ToDictionary(
            control => control.ClrTypeName,
            StringComparer.Ordinal);
        _unsupportedByClrTypeName = unsupportedByClrTypeName ??
            new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public IReadOnlyList<ControlMetadataModel> ExternalControls => _externalByClrTypeName.Values.ToList();

    public bool TryGetBuiltIn(string tagName, out ControlMetadataModel? control)
    {
        return ControlMetadataRegistry.TryGetControl(tagName, out control);
    }

    public bool TryGetBuiltInByClrTypeName(string clrTypeName, out ControlMetadataModel? control)
    {
        return _builtInsByClrTypeName.TryGetValue(clrTypeName, out control);
    }

    public bool TryGetExternalByClrTypeName(string clrTypeName, out ControlMetadataModel? control)
    {
        return _externalByClrTypeName.TryGetValue(clrTypeName, out control);
    }

    public bool TryGetUnsupportedReason(string clrTypeName, out string? reason)
    {
        return _unsupportedByClrTypeName.TryGetValue(clrTypeName, out reason);
    }
}
