using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.CSharp;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Hover;

/// <summary>
/// Provides hover information for CSXAML markup and embedded C# expressions.
/// </summary>
public sealed class CsxamlHoverService
{
    private readonly CsxamlCSharpHoverService _csharpHoverService = new();
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    /// <summary>
    /// Gets hover information for a source position.
    /// </summary>
    /// <param name="filePath">The CSXAML file path.</param>
    /// <param name="text">The current CSXAML source text.</param>
    /// <param name="position">The zero-based source offset to inspect.</param>
    /// <returns>The hover information, or <see langword="null"/> when none is available.</returns>
    public CsxamlHoverInfo? GetHover(string filePath, string text, int position)
    {
        var markup = CsxamlMarkupScanner.Scan(text);
        var targetElement = FindElementAtPosition(markup, position);
        var targetAttribute = FindAttributeAtPosition(markup, position);
        if (targetElement is null && targetAttribute is null)
        {
            var keywordHover = CsxamlKeywordHoverService.TryGetHover(text, position);
            if (keywordHover is not null)
            {
                return keywordHover;
            }

            return _csharpHoverService.GetHover(filePath, text, position);
        }

        var workspace = _workspaceLoader.Load(filePath, text);
        var currentNamespace = markup.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;
        var markupHover = targetAttribute is not null
            ? GetAttributeHover(markup, currentNamespace, workspace, targetAttribute.Value.Element, targetAttribute.Value.Attribute)
            : GetTagHover(currentNamespace, markup, workspace, targetElement!, position);
        return markupHover ?? _csharpHoverService.GetHover(filePath, text, position);
    }

    private CsxamlHoverInfo? GetTagHover(
        string currentNamespace,
        CsxamlMarkupScanResult markup,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element,
        int position)
    {
        if (element.PropertyContentName is not null &&
            position >= element.PropertyContentNameStart &&
            position <= element.PropertyContentNameStart + element.PropertyContentNameLength)
        {
            return GetPropertyContentHover(currentNamespace, markup, workspace, element);
        }

        var resolvedTag = _tagResolver.Resolve(element.TagName, markup.UsingDirectives, currentNamespace, workspace);
        var markdown = resolvedTag.Kind switch
        {
            CsxamlResolvedTagKind.Component when resolvedTag.Component is not null
                => CsxamlHoverFormatter.FormatComponentTag(resolvedTag.Component),
            CsxamlResolvedTagKind.Native when resolvedTag.Control is not null
                => CsxamlHoverFormatter.FormatControlTag(resolvedTag.Control, "WinUI control"),
            CsxamlResolvedTagKind.External when resolvedTag.Control is not null
                => CsxamlHoverFormatter.FormatControlTag(resolvedTag.Control, "External control"),
            _ => null,
        };

        return markdown is null
            ? null
            : new CsxamlHoverInfo(element.NameStart, element.NameLength, markdown);
    }

    private CsxamlHoverInfo? GetPropertyContentHover(
        string currentNamespace,
        CsxamlMarkupScanResult markup,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element)
    {
        if (element.PropertyContentOwner is null || element.PropertyContentName is null)
        {
            return null;
        }

        var resolvedOwner = _tagResolver.Resolve(
            element.PropertyContentOwner,
            markup.UsingDirectives,
            currentNamespace,
            workspace);
        if (resolvedOwner.Control is not null)
        {
            return GetNativePropertyContentHover(element, resolvedOwner.Control);
        }

        if (resolvedOwner.Component is not null &&
            resolvedOwner.Component.Metadata.NamedSlots.Any(slot => slot.Name == element.PropertyContentName))
        {
            return new CsxamlHoverInfo(
                element.PropertyContentNameStart,
                element.PropertyContentNameLength,
                CsxamlHoverFormatter.FormatComponentNamedSlot(
                    resolvedOwner.Component,
                    element.PropertyContentName));
        }

        return null;
    }

    private static CsxamlHoverInfo? GetNativePropertyContentHover(
        CsxamlMarkupElementReference element,
        Csxaml.ControlMetadata.ControlMetadata control)
    {
        var propertyName = element.PropertyContentName!;
        if (string.Equals(control.Content.DefaultPropertyName, propertyName, StringComparison.Ordinal))
        {
            return new CsxamlHoverInfo(
                element.PropertyContentNameStart,
                element.PropertyContentNameLength,
                CsxamlHoverFormatter.FormatNativePropertyContent(
                    control,
                    propertyName,
                    control.Content.Kind,
                    control.Content.PropertyTypeName));
        }

        var property = control.Properties.FirstOrDefault(candidate => candidate.Name == propertyName);
        return property is null
            ? null
            : new CsxamlHoverInfo(
                element.PropertyContentNameStart,
                element.PropertyContentNameLength,
                CsxamlHoverFormatter.FormatNativePropertyContent(
                    control,
                    property.Name,
                    ControlContentKind.Single,
                    property.ClrTypeName));
    }

    private CsxamlHoverInfo? GetAttributeHover(
        CsxamlMarkupScanResult markup,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element,
        CsxamlMarkupAttributeReference attribute)
    {
        if (TryGetAttachedPropertyHover(attribute, out var attachedHover))
        {
            return attachedHover;
        }

        var resolvedTag = _tagResolver.Resolve(element.TagName, markup.UsingDirectives, currentNamespace, workspace);
        if (resolvedTag.Control is not null)
        {
            var control = resolvedTag.Control;
            if (string.Equals(attribute.Name, "Ref", StringComparison.Ordinal))
            {
                return new CsxamlHoverInfo(attribute.Start, attribute.Length, CsxamlHoverFormatter.FormatElementRef(control));
            }

            var @event = control.Events.FirstOrDefault(candidate => string.Equals(candidate.ExposedName, attribute.Name, StringComparison.Ordinal));
            if (@event is not null)
            {
                return new CsxamlHoverInfo(attribute.Start, attribute.Length, CsxamlHoverFormatter.FormatEvent(control, @event));
            }

            var property = control.Properties.FirstOrDefault(candidate => string.Equals(candidate.Name, attribute.Name, StringComparison.Ordinal));
            if (property is not null)
            {
                return new CsxamlHoverInfo(attribute.Start, attribute.Length, CsxamlHoverFormatter.FormatProperty(control, property));
            }
        }

        if (resolvedTag.Component is not null)
        {
            var parameter = resolvedTag.Component.Metadata.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Name, attribute.Name, StringComparison.Ordinal));
            if (parameter is not null)
            {
                return new CsxamlHoverInfo(attribute.Start, attribute.Length, CsxamlHoverFormatter.FormatComponentParameter(resolvedTag.Component, parameter));
            }
        }

        return null;
    }

    private static bool TryGetAttachedPropertyHover(
        CsxamlMarkupAttributeReference attribute,
        out CsxamlHoverInfo? hover)
    {
        var separatorIndex = attribute.Name.IndexOf('.');
        if (separatorIndex <= 0 ||
            !AttachedPropertyMetadataRegistry.TryGetProperty(
                attribute.Name[..separatorIndex],
                attribute.Name[(separatorIndex + 1)..],
                out var property))
        {
            hover = null;
            return false;
        }

        hover = new CsxamlHoverInfo(attribute.Start, attribute.Length, CsxamlHoverFormatter.FormatAttachedProperty(property!));
        return true;
    }

    private static CsxamlMarkupElementReference? FindElementAtPosition(CsxamlMarkupScanResult markup, int position)
    {
        return markup.Elements.FirstOrDefault(
            candidate => position >= candidate.NameStart && position <= candidate.NameStart + candidate.NameLength);
    }

    private static (CsxamlMarkupElementReference Element, CsxamlMarkupAttributeReference Attribute)? FindAttributeAtPosition(
        CsxamlMarkupScanResult markup,
        int position)
    {
        foreach (var element in markup.Elements.Where(candidate => !candidate.IsClosing))
        {
            var attribute = element.Attributes.FirstOrDefault(
                candidate => position >= candidate.Start && position <= candidate.Start + candidate.Length);
            if (attribute is not null)
            {
                return (element, attribute);
            }
        }

        return null;
    }
}
