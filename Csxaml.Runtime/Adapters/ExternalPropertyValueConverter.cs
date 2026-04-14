using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal static class ExternalPropertyValueConverter
{
    public static object? Convert(NativePropertyValue property, Type targetType)
    {
        if (TryConvert(property.Value, targetType, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Native property '{property.Name}' expected a value assignable to '{targetType.Name}'.");
    }

    private static bool TryConvert(object? value, Type targetType, out object? converted)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (value is null)
        {
            converted = null;
            return !targetType.IsValueType || underlyingType is not null;
        }

        var effectiveType = underlyingType ?? targetType;
        if (typeof(Style).IsAssignableFrom(effectiveType))
        {
            converted = StyleValueResolver.Resolve(value);
            return true;
        }

        if (effectiveType.IsInstanceOfType(value) || effectiveType == typeof(object))
        {
            converted = value;
            return true;
        }

        if (typeof(Brush).IsAssignableFrom(effectiveType))
        {
            converted = BrushValueConverter.Convert(value);
            return true;
        }

        if (effectiveType == typeof(bool) && value is bool boolValue)
        {
            converted = boolValue;
            return true;
        }

        if (effectiveType == typeof(int) && TryReadInt(value, out var intValue))
        {
            converted = intValue;
            return true;
        }

        if (effectiveType == typeof(double) && TryReadDouble(value, out var doubleValue))
        {
            converted = doubleValue;
            return true;
        }

        if (effectiveType == typeof(Thickness) && TryReadThickness(value, out var thickness))
        {
            converted = thickness;
            return true;
        }

        if (effectiveType.IsEnum && value.GetType() == effectiveType)
        {
            converted = value;
            return true;
        }

        converted = null;
        return false;
    }

    private static bool TryReadInt(object value, out int converted)
    {
        switch (value)
        {
            case byte byteValue:
                converted = byteValue;
                return true;
            case short shortValue:
                converted = shortValue;
                return true;
            case int intValue:
                converted = intValue;
                return true;
            case long longValue when longValue is >= int.MinValue and <= int.MaxValue:
                converted = (int)longValue;
                return true;
            default:
                converted = default;
                return false;
        }
    }

    private static bool TryReadDouble(object value, out double converted)
    {
        switch (value)
        {
            case byte byteValue:
                converted = byteValue;
                return true;
            case short shortValue:
                converted = shortValue;
                return true;
            case int intValue:
                converted = intValue;
                return true;
            case long longValue:
                converted = longValue;
                return true;
            case float floatValue:
                converted = floatValue;
                return true;
            case double doubleValue:
                converted = doubleValue;
                return true;
            case decimal decimalValue:
                converted = (double)decimalValue;
                return true;
            default:
                converted = default;
                return false;
        }
    }

    private static bool TryReadThickness(object value, out Thickness thickness)
    {
        if (value is Thickness typedThickness)
        {
            thickness = typedThickness;
            return true;
        }

        if (TryReadDouble(value, out var uniform))
        {
            thickness = new Thickness(uniform);
            return true;
        }

        thickness = default;
        return false;
    }
}
