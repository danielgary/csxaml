using BenchmarkDotNet.Attributes;

namespace Csxaml.Benchmarks.Metadata;

public class ControlLookupBenchmarks
{
    private static readonly IReadOnlyList<string> ImportedNamespaces =
    [
        "Microsoft.UI.Xaml.Controls",
        "Microsoft.UI.Xaml.Automation",
    ];
    private const string ExternalTagName = "StatusButton";

    [GlobalSetup]
    public void Setup()
    {
        ExternalControlRegistry.ClearForTests();
        ExternalControlRegistry.Register(
            new ExternalControlDescriptor(
                typeof(object),
                new ControlMetadataModel(
                    ExternalTagName,
                    "Csxaml.ExternalControls.StatusButton",
                    "Microsoft.UI.Xaml.Controls.Button",
                    ControlChildKind.None,
                    Array.Empty<PropertyMetadata>(),
                    Array.Empty<EventMetadata>())));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [Benchmark]
    public ControlMetadataModel GetBuiltInTextBox()
    {
        return ControlMetadataRegistry.GetControl("TextBox");
    }

    [Benchmark]
    public AttachedPropertyResolutionResult ResolveAutomationName()
    {
        return AttachedPropertyReferenceResolver.Resolve(
            "AutomationProperties",
            "Name",
            currentNamespace: "Csxaml.Demo.Components",
            ImportedNamespaces,
            Array.Empty<KeyValuePair<string, string>>());
    }

    [Benchmark]
    public ExternalControlDescriptor GetExternalStatusButton()
    {
        ExternalControlRegistry.TryGet(ExternalTagName, out var descriptor);
        return descriptor!;
    }
}
