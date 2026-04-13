using System.Globalization;

namespace Csxaml.ControlMetadata.Generator;

internal sealed class MetadataSourceEmitter
{
    public string Emit(IReadOnlyList<ControlMetadata> controls)
    {
        var writer = new IndentedSourceWriter();
        writer.WriteLine("namespace Csxaml.ControlMetadata;");
        writer.WriteLine();
        writer.WriteLine("internal static class GeneratedControlMetadata");
        writer.WriteLine("{");
        writer.PushIndent();
        writer.WriteLine("public static IReadOnlyList<ControlMetadata> All { get; } =");
        writer.WriteLine("[");
        writer.PushIndent();

        foreach (var control in controls)
        {
            EmitControl(writer, control);
        }

        writer.PopIndent();
        writer.WriteLine("];");
        writer.PopIndent();
        writer.WriteLine("}");
        return writer.ToString();
    }

    private static void EmitControl(IndentedSourceWriter writer, ControlMetadata control)
    {
        writer.WriteLine("new ControlMetadata(");
        writer.PushIndent();
        writer.WriteLine($"{Quote(control.TagName)},");
        writer.WriteLine($"{Quote(control.ClrTypeName)},");
        writer.WriteLine(control.BaseTypeName is null ? "null," : $"{Quote(control.BaseTypeName)},");
        writer.WriteLine($"ControlChildKind.{control.ChildKind},");
        EmitProperties(writer, control.Properties);
        EmitEvents(writer, control.Events);
        writer.PopIndent();
        writer.WriteLine("),");
    }

    private static void EmitEvents(IndentedSourceWriter writer, IReadOnlyList<EventMetadata> events)
    {
        writer.WriteLine("[");
        writer.PushIndent();
        foreach (var eventMetadata in events)
        {
            writer.WriteLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"new EventMetadata({QuoteOrNull(eventMetadata.ClrEventName)}, {Quote(eventMetadata.ExposedName)}, {Quote(eventMetadata.HandlerTypeName)}, {eventMetadata.ExposedInCsxaml.ToString().ToLowerInvariant()}, ValueKindHint.{eventMetadata.ValueKindHint}, EventBindingKind.{eventMetadata.BindingKind}),"));
        }

        writer.PopIndent();
        writer.WriteLine("]");
    }

    private static void EmitProperties(
        IndentedSourceWriter writer,
        IReadOnlyList<PropertyMetadata> properties)
    {
        writer.WriteLine("[");
        writer.PushIndent();
        foreach (var property in properties)
        {
            writer.WriteLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"new PropertyMetadata({Quote(property.Name)}, {Quote(property.ClrTypeName)}, {property.IsWritable.ToString().ToLowerInvariant()}, {property.IsDependencyProperty.ToString().ToLowerInvariant()}, {property.IsAttached.ToString().ToLowerInvariant()}, {property.ExposedInCsxaml.ToString().ToLowerInvariant()}, ValueKindHint.{property.ValueKindHint}),"));
        }

        writer.PopIndent();
        writer.WriteLine("],");
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static string QuoteOrNull(string? value)
    {
        return value is null ? "null" : Quote(value);
    }
}
