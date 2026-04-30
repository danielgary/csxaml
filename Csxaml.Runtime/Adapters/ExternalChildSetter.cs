using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class ExternalChildSetter
{
    private readonly ControlContentMetadata _content;
    private readonly PropertyInfo? _childrenProperty;
    private readonly PropertyInfo? _contentProperty;

    private ExternalChildSetter(
        ControlContentMetadata content,
        PropertyInfo? contentProperty,
        PropertyInfo? childrenProperty)
    {
        _content = content;
        _contentProperty = contentProperty;
        _childrenProperty = childrenProperty;
    }

    public void Set(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        switch (_content.Kind)
        {
            case ControlContentKind.None:
                SetNoChildren(children);
                return;
            case ControlContentKind.Single:
                SetSingleChild(element, children);
                return;
            case ControlContentKind.Collection:
                SetMultipleChildren(element, children);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported content kind '{_content.Kind}'.");
        }
    }

    public static ExternalChildSetter Create(ExternalControlDescriptor descriptor)
    {
        return new ExternalChildSetter(
            descriptor.Metadata.Content,
            FindSingleChildProperty(descriptor.ControlType, descriptor.Metadata.Content),
            FindCollectionChildProperty(descriptor.ControlType, descriptor.Metadata.Content));
    }

    private static PropertyInfo? FindCollectionChildProperty(
        Type controlType,
        ControlContentMetadata content)
    {
        if (content.Kind != ControlContentKind.Collection)
        {
            return null;
        }

        return FindNamedProperty(controlType, content.DefaultPropertyName) ??
            FindNamedProperty(controlType, "Children");
    }

    private static PropertyInfo? FindSingleChildProperty(
        Type controlType,
        ControlContentMetadata content)
    {
        if (content.Kind != ControlContentKind.Single)
        {
            return null;
        }

        return FindNamedProperty(controlType, content.DefaultPropertyName) ??
            FindNamedProperty(controlType, "Child") ??
            FindNamedProperty(controlType, "Content");
    }

    private static PropertyInfo? FindNamedProperty(Type controlType, string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
            ? null
            : controlType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
    }

    private void SetNoChildren(IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            if (_content.DefaultPropertyName is not null)
            {
                throw new InvalidOperationException(
                    $"External control content property '{_content.DefaultPropertyName}' has unsupported type '{_content.PropertyTypeName ?? "unknown"}'.");
            }

            throw new InvalidOperationException("External control does not support child content.");
        }
    }

    private void SetSingleChild(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        if (_contentProperty is null)
        {
            throw new InvalidOperationException(
                $"External control is missing single-child property '{_content.DefaultPropertyName ?? "Child/Content"}'.");
        }

        if (children.Count > 1)
        {
            throw new InvalidOperationException(
                $"External control supports only one child for '{_contentProperty.Name}'.");
        }

        _contentProperty.SetValue(element, children.SingleOrDefault());
    }

    private void SetMultipleChildren(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        if (_childrenProperty?.GetValue(element) is not UIElementCollection collection)
        {
            throw new InvalidOperationException(
                $"External control is missing UIElementCollection property '{_content.DefaultPropertyName ?? "Children"}'.");
        }

        UiElementCollectionPatcher.Update(collection, children);
    }
}
