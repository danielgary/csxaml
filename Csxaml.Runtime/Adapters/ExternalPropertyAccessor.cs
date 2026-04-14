using System.Reflection;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal sealed class ExternalPropertyAccessor
{
    private readonly object? _defaultValue;
    private readonly DependencyProperty? _dependencyProperty;
    private readonly PropertyInfo _property;
    private readonly Type _propertyType;

    private ExternalPropertyAccessor(
        string name,
        Type propertyType,
        PropertyInfo property,
        DependencyProperty? dependencyProperty,
        object? defaultValue)
    {
        Name = name;
        _propertyType = propertyType;
        _property = property;
        _dependencyProperty = dependencyProperty;
        _defaultValue = defaultValue;
    }

    public string Name { get; }

    public void Apply(object element, NativeElementNode node)
    {
        var nodeProperty = node.Properties.SingleOrDefault(
            candidate => string.Equals(candidate.Name, Name, StringComparison.Ordinal));
        if (nodeProperty is null)
        {
            ClearValue(element);
            return;
        }

        var converted = ExternalPropertyValueConverter.Convert(nodeProperty, _propertyType);
        if (_dependencyProperty is not null)
        {
            ((DependencyObject)element).SetValue(_dependencyProperty, converted);
            return;
        }

        _property.SetValue(element, converted);
    }

    public static ExternalPropertyAccessor Create(
        Type controlType,
        Csxaml.ControlMetadata.PropertyMetadata propertyMetadata)
    {
        var property = controlType.GetProperty(propertyMetadata.Name, BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"External control '{controlType.FullName}' is missing property '{propertyMetadata.Name}'.");
        var dependencyProperty = propertyMetadata.IsDependencyProperty
            ? FindDependencyProperty(controlType, propertyMetadata.Name)
            : null;
        var defaultValue = propertyMetadata.IsDependencyProperty
            ? null
            : ReadDefaultValue(controlType, property);
        return new ExternalPropertyAccessor(
            propertyMetadata.Name,
            property.PropertyType,
            property,
            dependencyProperty,
            defaultValue);
    }

    private void ClearValue(object element)
    {
        if (_dependencyProperty is not null)
        {
            ((DependencyObject)element).ClearValue(_dependencyProperty);
            return;
        }

        _property.SetValue(element, _defaultValue);
    }

    private static object? ReadDefaultValue(Type controlType, PropertyInfo property)
    {
        var instance = Activator.CreateInstance(controlType) ??
            throw new InvalidOperationException(
                $"External control '{controlType.FullName}' must create a default instance for property '{property.Name}'.");
        return property.GetValue(instance);
    }

    private static DependencyProperty FindDependencyProperty(Type controlType, string propertyName)
    {
        for (var current = controlType; current is not null; current = current.BaseType)
        {
            var field = current.GetField(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field?.GetValue(null) is DependencyProperty dependencyProperty)
            {
                return dependencyProperty;
            }

            var property = current.GetProperty(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (property?.GetValue(null) is DependencyProperty dependencyPropertyValue)
            {
                return dependencyPropertyValue;
            }
        }

        throw new InvalidOperationException(
            $"External control '{controlType.FullName}' is missing dependency property '{propertyName}Property'.");
    }
}
