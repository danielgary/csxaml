using Csxaml.ControlMetadata;

namespace Csxaml.Generator;

internal sealed class GeneratedComponentManifestEmitter
{
    private readonly IndentedCodeWriter _writer;

    public GeneratedComponentManifestEmitter(IndentedCodeWriter writer)
    {
        _writer = writer;
    }

    public void Emit(CompilationContext compilation, IReadOnlyList<ComponentCatalogEntry> localComponents)
    {
        EmitUsings();
        EmitAssemblyAttribute(compilation.Project.InternalGeneratedNamespace);
        EmitNamespace(compilation.Project.InternalGeneratedNamespace);
        EmitProviderClass(localComponents);
    }

    private static string BuildComponentsExpression(IReadOnlyList<ComponentCatalogEntry> localComponents)
    {
        if (localComponents.Count == 0)
        {
            return "Array.Empty<ComponentMetadata>()";
        }

        var values = localComponents
            .OrderBy(component => component.NamespaceName, StringComparer.Ordinal)
            .ThenBy(component => component.Name, StringComparer.Ordinal)
            .Select(
                component =>
                    $"new ComponentMetadata({FormatStringLiteral(component.Name)}, {FormatStringLiteral(component.NamespaceName)}, {FormatStringLiteral(component.AssemblyName)}, {FormatStringLiteral(component.ComponentTypeName)}, {FormatNullableStringLiteral(component.PropsTypeName)}, {BuildParametersExpression(component.Parameters)}, {FormatBool(component.SupportsDefaultSlot)}, {BuildSlotsExpression(component.NamedSlots)}, ComponentKind.{component.Kind})");
        return $"new ComponentMetadata[] {{ {string.Join(", ", values)} }}";
    }

    private static string BuildSlotsExpression(IReadOnlyList<ComponentSlotMetadata> namedSlots)
    {
        if (namedSlots.Count == 0)
        {
            return "Array.Empty<ComponentSlotMetadata>()";
        }

        var values = namedSlots
            .Select(slot => $"new ComponentSlotMetadata({FormatStringLiteral(slot.Name)})");
        return $"new ComponentSlotMetadata[] {{ {string.Join(", ", values)} }}";
    }

    private static string BuildParametersExpression(IReadOnlyList<ComponentParameterMetadata> parameters)
    {
        if (parameters.Count == 0)
        {
            return "Array.Empty<ComponentParameterMetadata>()";
        }

        var values = parameters
            .Select(
                parameter =>
                    $"new ComponentParameterMetadata({FormatStringLiteral(parameter.Name)}, {FormatStringLiteral(parameter.TypeName)})");
        return $"new ComponentParameterMetadata[] {{ {string.Join(", ", values)} }}";
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

    private void EmitAssemblyAttribute(string generatedNamespace)
    {
        _writer.WriteLine(
            $"[assembly: CsxamlComponentManifestProviderAttribute(typeof(global::{generatedNamespace}.GeneratedComponentManifestProvider))]");
        _writer.WriteLine();
    }

    private void EmitNamespace(string namespaceName)
    {
        _writer.WriteLine($"namespace {namespaceName};");
        _writer.WriteLine();
    }

    private void EmitProviderClass(IReadOnlyList<ComponentCatalogEntry> localComponents)
    {
        _writer.WriteLine("internal sealed class GeneratedComponentManifestProvider : IComponentManifestProvider");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("public CompiledComponentManifest GetManifest()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine(
            $"return new CompiledComponentManifest({BuildComponentsExpression(localComponents)});");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitUsings()
    {
        _writer.WriteLine("using System;");
        _writer.WriteLine("using Csxaml.ControlMetadata;");
        _writer.WriteLine();
    }
}
