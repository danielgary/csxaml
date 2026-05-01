using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class NativePropertyContentSetter
{
    public static void Set(
        object element,
        string propertyName,
        IReadOnlyList<object> children)
    {
        var property = element.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
        {
            throw new InvalidOperationException(
                $"Projected element '{element.GetType().Name}' is missing property '{propertyName}'.");
        }

        if (IsCollectionProperty(property))
        {
            SetCollection(element, property, children);
            return;
        }

        SetSingle(element, property, children);
    }

    private static bool IsCollectionProperty(PropertyInfo property)
    {
        return string.Equals(
            property.PropertyType.FullName,
            typeof(UIElementCollection).FullName,
            StringComparison.Ordinal);
    }

    private static UIElement[] RequireUiElements(
        string propertyName,
        IReadOnlyList<object> children)
    {
        var uiChildren = new UIElement[children.Count];
        for (var index = 0; index < children.Count; index++)
        {
            if (children[index] is not UIElement uiElement)
            {
                throw new InvalidOperationException(
                    $"Property content for '{propertyName}' must project UIElement children.");
            }

            uiChildren[index] = uiElement;
        }

        return uiChildren;
    }

    private static void SetCollection(
        object element,
        PropertyInfo property,
        IReadOnlyList<object> children)
    {
        if (property.GetValue(element) is not UIElementCollection collection)
        {
            throw new InvalidOperationException(
                $"Property '{property.Name}' must return a UIElementCollection.");
        }

        UiElementCollectionPatcher.Update(collection, RequireUiElements(property.Name, children));
    }

    private static void SetSingle(
        object element,
        PropertyInfo property,
        IReadOnlyList<object> children)
    {
        if (children.Count > 1)
        {
            throw new InvalidOperationException(
                $"Property '{property.Name}' supports only one property-content child.");
        }

        var child = children.SingleOrDefault();
        if (child is not null && !property.PropertyType.IsInstanceOfType(child) && property.PropertyType != typeof(object))
        {
            throw new InvalidOperationException(
                $"Property '{property.Name}' cannot receive '{child.GetType().Name}'.");
        }

        if (ReferenceEquals(property.GetValue(element), child))
        {
            return;
        }

        property.SetValue(element, child);
    }
}
