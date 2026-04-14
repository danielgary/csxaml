using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class ExternalChildSetter
{
    private readonly ControlChildKind _childKind;
    private readonly PropertyInfo? _childrenProperty;
    private readonly PropertyInfo? _contentProperty;

    private ExternalChildSetter(
        ControlChildKind childKind,
        PropertyInfo? contentProperty,
        PropertyInfo? childrenProperty)
    {
        _childKind = childKind;
        _contentProperty = contentProperty;
        _childrenProperty = childrenProperty;
    }

    public void Set(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        switch (_childKind)
        {
            case ControlChildKind.None:
                SetNoChildren(children);
                return;
            case ControlChildKind.Single:
                SetSingleChild(element, children);
                return;
            case ControlChildKind.Multiple:
                SetMultipleChildren(element, children);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported child kind '{_childKind}'.");
        }
    }

    public static ExternalChildSetter Create(ExternalControlDescriptor descriptor)
    {
        return new ExternalChildSetter(
            descriptor.Metadata.ChildKind,
            FindContentProperty(descriptor.ControlType),
            FindChildrenProperty(descriptor.ControlType));
    }

    private static PropertyInfo? FindChildrenProperty(Type controlType)
    {
        return controlType.GetProperty("Children", BindingFlags.Instance | BindingFlags.Public);
    }

    private static PropertyInfo? FindContentProperty(Type controlType)
    {
        return controlType.GetProperty("Child", BindingFlags.Instance | BindingFlags.Public) ??
            controlType.GetProperty("Content", BindingFlags.Instance | BindingFlags.Public);
    }

    private static void SetNoChildren(IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("External control does not support child content.");
        }
    }

    private void SetSingleChild(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        if (_contentProperty is null)
        {
            throw new InvalidOperationException("External control is missing a single-child property.");
        }

        if (children.Count > 1)
        {
            throw new InvalidOperationException("External control supports only one child.");
        }

        _contentProperty.SetValue(element, children.SingleOrDefault());
    }

    private void SetMultipleChildren(FrameworkElement element, IReadOnlyList<UIElement> children)
    {
        if (_childrenProperty?.GetValue(element) is not UIElementCollection collection)
        {
            throw new InvalidOperationException("External control is missing a UIElementCollection children property.");
        }

        UiElementCollectionPatcher.Update(collection, children);
    }
}
