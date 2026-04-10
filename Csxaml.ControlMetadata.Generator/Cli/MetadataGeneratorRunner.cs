namespace Csxaml.ControlMetadata.Generator;

internal sealed class MetadataGeneratorRunner
{
    private readonly ControlMetadataDiscoverer _discoverer = new();
    private readonly MetadataSourceEmitter _emitter = new();
    private readonly SupportedControlFilter _filter = new();

    public void Generate(MetadataGeneratorOptions options)
    {
        var controls = CuratedControlSet.Definitions
            .Select(definition => _filter.BuildMetadata(definition, _discoverer.Discover(definition.ControlType)))
            .OrderBy(control => control.TagName, StringComparer.Ordinal)
            .ToList();

        var source = _emitter.Emit(controls);
        Directory.CreateDirectory(Path.GetDirectoryName(options.OutputPath)!);
        File.WriteAllText(options.OutputPath, source);
    }
}
