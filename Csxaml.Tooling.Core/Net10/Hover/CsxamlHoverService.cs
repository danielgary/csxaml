using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.CSharp;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Hover;

public sealed class CsxamlHoverService
{
    private readonly CsxamlCSharpHoverService _csharpHoverService = new();
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    public CsxamlHoverInfo? GetHover(string filePath, string text, int position)
    {
        var markup = CsxamlMarkupScanner.Scan(text);
        var targetElement = FindElementAtPosition(markup, position);
        var targetAttribute = FindAttributeAtPosition(markup, position);
        if (targetElement is null && targetAttribute is null)
        {
            return _csharpHoverService.GetHover(filePath, text, position);
        }

        var workspace = _workspaceLoader.Load(filePath, text);
        var currentNamespace = markup.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;
        var markupHover = targetAttribute is not null
            ? GetAttributeHover(markup, currentNamespace, workspace, targetAttribute.Value.Element, targetAttribute.Value.Attribute)
            : GetTagHover(currentNamespace, markup, workspace, targetElement!);
        return markupHover ?? _csharpHoverService.GetHover(filePath, text, position);
    }

    private CsxamlHoverInfo? GetTagHover(
        string currentNamespace,
        CsxamlMarkupScanResult markup,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element)
    {
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
