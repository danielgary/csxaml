namespace Csxaml.Generator;

internal sealed class GeneratedExternalControlRegistrationEmitter
{
    private readonly IndentedCodeWriter _writer;

    public GeneratedExternalControlRegistrationEmitter(IndentedCodeWriter writer)
    {
        _writer = writer;
    }

    public void Emit(CompilationContext compilation)
    {
        EmitUsings();
        EmitNamespace(compilation.Project.InternalGeneratedNamespace);
        EmitRegistrationClass(compilation.NativeControls.ExternalControls);
    }

    private void EmitNamespace(string namespaceName)
    {
        _writer.WriteLine($"namespace {namespaceName};");
        _writer.WriteLine();
    }

    private void EmitRegistrationClass(IReadOnlyList<ControlMetadataModel> controls)
    {
        _writer.WriteLine("internal static class GeneratedExternalControlRegistration");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("private static bool _isRegistered;");
        _writer.WriteLine();
        _writer.WriteLine("public static void EnsureRegistered()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("if (_isRegistered)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("return;");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
        _writer.WriteLine("_isRegistered = true;");
        foreach (var control in controls.OrderBy(control => control.ClrTypeName, StringComparer.Ordinal))
        {
            _writer.WriteLine();
            EmitRegisterCall(control);
        }
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitRegisterCall(ControlMetadataModel control)
    {
        _writer.WriteLine("global::Csxaml.Runtime.ExternalControlRegistry.Register(");
        _writer.PushIndent();
        _writer.WriteLine("new global::Csxaml.Runtime.ExternalControlDescriptor(");
        _writer.PushIndent();
        _writer.WriteLine($"typeof({FormatTypeLiteral(control.ClrTypeName)}),");
        _writer.WriteLine("new global::Csxaml.ControlMetadata.ControlMetadata(");
        _writer.PushIndent();
        _writer.WriteLine($"{FormatStringLiteral(control.TagName)},");
        _writer.WriteLine($"{FormatStringLiteral(control.ClrTypeName)},");
        _writer.WriteLine($"{FormatNullableStringLiteral(control.BaseTypeName)},");
        _writer.WriteLine($"global::Csxaml.ControlMetadata.ControlChildKind.{control.ChildKind},");
        _writer.WriteLine($"{BuildPropertiesExpression(control.Properties)},");
        _writer.WriteLine($"{BuildEventsExpression(control.Events)})");
        _writer.PopIndent();
        _writer.WriteLine("));");
        _writer.PopIndent();
        _writer.PopIndent();
    }

    private static string BuildEventsExpression(IReadOnlyList<EventMetadata> events)
    {
        if (events.Count == 0)
        {
            return "Array.Empty<global::Csxaml.ControlMetadata.EventMetadata>()";
        }

        var values = events
            .OrderBy(eventMetadata => eventMetadata.ExposedName, StringComparer.Ordinal)
            .Select(
                eventMetadata =>
                    $"new global::Csxaml.ControlMetadata.EventMetadata({FormatNullableStringLiteral(eventMetadata.ClrEventName)}, {FormatStringLiteral(eventMetadata.ExposedName)}, {FormatStringLiteral(eventMetadata.HandlerTypeName)}, {FormatBool(eventMetadata.ExposedInCsxaml)}, global::Csxaml.ControlMetadata.ValueKindHint.{eventMetadata.ValueKindHint}, global::Csxaml.ControlMetadata.EventBindingKind.{eventMetadata.BindingKind})");
        return $"new global::Csxaml.ControlMetadata.EventMetadata[] {{ {string.Join(", ", values)} }}";
    }

    private static string BuildPropertiesExpression(IReadOnlyList<PropertyMetadata> properties)
    {
        if (properties.Count == 0)
        {
            return "Array.Empty<global::Csxaml.ControlMetadata.PropertyMetadata>()";
        }

        var values = properties
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(
                property =>
                    $"new global::Csxaml.ControlMetadata.PropertyMetadata({FormatStringLiteral(property.Name)}, {FormatStringLiteral(property.ClrTypeName)}, {FormatBool(property.IsWritable)}, {FormatBool(property.IsDependencyProperty)}, {FormatBool(property.IsAttached)}, {FormatBool(property.ExposedInCsxaml)}, global::Csxaml.ControlMetadata.ValueKindHint.{property.ValueKindHint})");
        return $"new global::Csxaml.ControlMetadata.PropertyMetadata[] {{ {string.Join(", ", values)} }}";
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatNullableStringLiteral(string? value)
    {
        return value is null ? "null" : FormatStringLiteral(value);
    }

    private static string FormatStringLiteral(string value)
    {
        return $"\"{EscapeString(value)}\"";
    }

    private static string FormatTypeLiteral(string clrTypeName)
    {
        return $"global::{clrTypeName.Replace("+", ".", StringComparison.Ordinal)}";
    }

    private void EmitUsings()
    {
        _writer.WriteLine("using System;");
        _writer.WriteLine("using Csxaml.ControlMetadata;");
        _writer.WriteLine("using Csxaml.Runtime;");
        _writer.WriteLine();
    }
}
