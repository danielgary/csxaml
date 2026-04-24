using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Projects;
using Microsoft.CodeAnalysis;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Hover;

internal static class CsxamlHoverFormatter
{
    private static readonly SymbolDisplayFormat CSharpMemberFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeExtensionThis |
                          SymbolDisplayParameterOptions.IncludeName |
                          SymbolDisplayParameterOptions.IncludeOptionalBrackets |
                          SymbolDisplayParameterOptions.IncludeParamsRefOut |
                          SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                              SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                              SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    private static readonly SymbolDisplayFormat CSharpTypeFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                              SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                              SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string FormatComponentTag(CsxamlWorkspaceComponentSymbol component)
    {
        var lines = new List<string>
        {
            "```cs",
            $"{component.Metadata.NamespaceName}.{component.Metadata.Name}",
            "```",
            "Component tag",
            string.Empty,
            $"- Assembly: `{component.Metadata.AssemblyName}`",
            $"- Type: `{component.Metadata.ComponentTypeName}`",
            $"- Parameters: {FormatParameters(component.Metadata.Parameters)}",
            $"- Default slot: {(component.Metadata.SupportsDefaultSlot ? "supported" : "not supported")}",
            $"- Definition: `{component.FilePath}`",
        };

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatControlTag(ControlMetadataModel control, string kindLabel)
    {
        var lines = new List<string>
        {
            "```cs",
            control.ClrTypeName,
            "```",
            $"{kindLabel} tag",
            string.Empty,
            $"- Children: {FormatChildKind(control.ChildKind)}",
            $"- Properties: {control.Properties.Count}",
            $"- Events: {control.Events.Count}",
        };

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatComponentParameter(
        CsxamlWorkspaceComponentSymbol component,
        ComponentParameterMetadata parameter)
    {
        var lines = new List<string>
        {
            "```cs",
            $"{component.Metadata.Name}.{parameter.Name}: {parameter.TypeName}",
            "```",
            "Component parameter",
            string.Empty,
            $"- Owner component: `{component.Metadata.NamespaceName}.{component.Metadata.Name}`",
            $"- Assembly: `{component.Metadata.AssemblyName}`",
        };

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatProperty(ControlMetadataModel control, PropertyMetadata property)
    {
        var lines = new List<string>
        {
            "```cs",
            $"{control.TagName}.{property.Name}: {property.ClrTypeName}",
            "```",
            "Native property",
            string.Empty,
            $"- Owner type: `{control.ClrTypeName}`",
            $"- Value kind: `{property.ValueKindHint}`",
            $"- Writable: {(property.IsWritable ? "yes" : "no")}",
            $"- Dependency property: {(property.IsDependencyProperty ? "yes" : "no")}",
        };

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatEvent(ControlMetadataModel control, EventMetadata @event)
    {
        var lines = new List<string>
        {
            "```cs",
            $"{control.TagName}.{@event.ExposedName}: {@event.HandlerTypeName}",
            "```",
            "Native event",
            string.Empty,
            $"- Owner type: `{control.ClrTypeName}`",
            $"- Binding kind: `{@event.BindingKind}`",
            $"- Value kind: `{@event.ValueKindHint}`",
        };

        if (!string.IsNullOrWhiteSpace(@event.ClrEventName))
        {
            lines.Add($"- CLR event: `{@event.ClrEventName}`");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatAttachedProperty(AttachedPropertyMetadata property)
    {
        var lines = new List<string>
        {
            "```cs",
            $"{property.QualifiedName}: {property.ClrTypeName}",
            "```",
            "Attached property",
            string.Empty,
            $"- Owner type: `{property.ClrOwnerTypeName}`",
            $"- Value kind: `{property.ValueKindHint}`",
        };

        if (!string.IsNullOrWhiteSpace(property.RequiredParentTagName))
        {
            lines.Add($"- Required parent: `<{property.RequiredParentTagName}>`");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatCSharpSymbol(ISymbol symbol)
    {
        var lines = new List<string>
        {
            "```cs",
            GetCSharpSymbolSignature(symbol),
            "```",
            GetCSharpSymbolLabel(symbol),
            string.Empty,
        };

        var containingType = GetContainingType(symbol);
        if (containingType is not null)
        {
            lines.Add($"- Containing type: `{containingType.ToDisplayString(CSharpTypeFormat)}`");
        }

        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            lines.Add($"- Namespace: `{symbol.ContainingNamespace.ToDisplayString()}`");
        }

        if (!string.IsNullOrWhiteSpace(symbol.ContainingAssembly?.Name))
        {
            lines.Add($"- Assembly: `{symbol.ContainingAssembly.Name}`");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatParameters(IReadOnlyList<ComponentParameterMetadata> parameters)
    {
        if (parameters.Count == 0)
        {
            return "none";
        }

        return string.Join(", ", parameters.Select(parameter => $"`{parameter.Name}: {parameter.TypeName}`"));
    }

    private static string FormatChildKind(ControlChildKind childKind)
    {
        return childKind switch
        {
            ControlChildKind.None => "no child content",
            ControlChildKind.Single => "single child",
            ControlChildKind.Multiple => "multiple children",
            _ => childKind.ToString(),
        };
    }

    private static string GetCSharpSymbolSignature(ISymbol symbol)
    {
        return symbol switch
        {
            ILocalSymbol local => $"{local.Type.ToDisplayString(CSharpTypeFormat)} {local.Name}",
            IParameterSymbol parameter => $"{parameter.Type.ToDisplayString(CSharpTypeFormat)} {parameter.Name}",
            INamedTypeSymbol type => type.ToDisplayString(CSharpTypeFormat),
            _ => symbol.ToDisplayString(CSharpMemberFormat),
        };
    }

    private static string GetCSharpSymbolLabel(ISymbol symbol)
    {
        return symbol switch
        {
            IEventSymbol => "C# event",
            IFieldSymbol => "C# field",
            ILocalSymbol => "C# local",
            IMethodSymbol => "C# method",
            INamedTypeSymbol namedType when namedType.TypeKind == TypeKind.Interface => "C# interface",
            INamedTypeSymbol => "C# type",
            INamespaceSymbol => "C# namespace",
            IParameterSymbol => "C# parameter",
            IPropertySymbol => "C# property",
            _ => "C# symbol",
        };
    }

    private static INamedTypeSymbol? GetContainingType(ISymbol symbol)
    {
        var containingType = symbol.ContainingType;
        if (containingType is null ||
            containingType.Name.StartsWith("__CsxamlProjection_", StringComparison.Ordinal))
        {
            return null;
        }

        return containingType;
    }
}
