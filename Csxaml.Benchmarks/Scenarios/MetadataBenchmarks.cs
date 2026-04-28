using BenchmarkDotNet.Attributes;
using Csxaml.ControlMetadata;

namespace Csxaml.Benchmarks.Scenarios;

[MemoryDiagnoser]
public class MetadataBenchmarks
{
    private static readonly string[] ControlNames =
    [
        "Border",
        "Button",
        "CheckBox",
        "Grid",
        "ScrollViewer",
        "StackPanel",
        "TextBlock",
        "TextBox"
    ];

    [Benchmark]
    public int BuiltInControlLookup()
    {
        var total = 0;
        foreach (var name in ControlNames)
        {
            total += ControlMetadataRegistry.GetControl(name).Properties.Count;
        }

        return total;
    }

    [Benchmark]
    public int AttachedPropertyLookup()
    {
        var total = 0;
        total += AttachedPropertyMetadataRegistry.GetProperty("Grid", "Row").ValueKindHint == ValueKindHint.Int ? 1 : 0;
        total += AttachedPropertyMetadataRegistry.GetProperty("Grid", "Column").ValueKindHint == ValueKindHint.Int ? 1 : 0;
        total += AttachedPropertyMetadataRegistry.GetProperty("AutomationProperties", "Name").ValueKindHint == ValueKindHint.String ? 1 : 0;
        total += AttachedPropertyMetadataRegistry.GetProperty("AutomationProperties", "AutomationId").ValueKindHint == ValueKindHint.String ? 1 : 0;
        return total;
    }
}
