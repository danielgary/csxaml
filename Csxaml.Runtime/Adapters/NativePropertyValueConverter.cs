namespace Csxaml.Runtime;

internal static class NativePropertyValueConverter
{
    public static bool TryConvert<T>(NativePropertyValue property, out T value)
    {
        if (property.Value is T typedValue)
        {
            value = typedValue;
            return true;
        }

        if (property.Value is null)
        {
            value = default!;
            return true;
        }

        if (TryConvertDouble(property, out value))
        {
            return true;
        }

        if (TryConvertBool(property, out value))
        {
            return true;
        }

        if (TryConvertNullableBool(property, out value))
        {
            return true;
        }

        value = default!;
        return false;
    }

    private static bool TryConvertBool<T>(NativePropertyValue property, out T value)
    {
        if (typeof(T) != typeof(bool) || property.ValueKindHint != ValueKindHint.Bool)
        {
            value = default!;
            return false;
        }

        if (!TryReadBool(property.Value, out var boolValue))
        {
            value = default!;
            return false;
        }

        value = (T)(object)boolValue;
        return true;
    }

    private static bool TryConvertDouble<T>(NativePropertyValue property, out T value)
    {
        if (typeof(T) != typeof(double) || property.ValueKindHint != ValueKindHint.Double)
        {
            value = default!;
            return false;
        }

        if (!TryReadDouble(property.Value!, out var number))
        {
            value = default!;
            return false;
        }

        value = (T)(object)number;
        return true;
    }

    private static bool TryConvertNullableBool<T>(NativePropertyValue property, out T value)
    {
        if (typeof(T) != typeof(bool?) || property.ValueKindHint != ValueKindHint.Bool)
        {
            value = default!;
            return false;
        }

        if (!TryReadBool(property.Value, out var boolValue))
        {
            value = default!;
            return false;
        }

        value = (T)(object)(bool?)boolValue;
        return true;
    }

    private static bool TryReadBool(object? value, out bool boolValue)
    {
        switch (value)
        {
            case bool typedValue:
                boolValue = typedValue;
                return true;
            default:
                boolValue = default;
                return false;
        }
    }

    private static bool TryReadDouble(object value, out double number)
    {
        switch (value)
        {
            case byte typedValue:
                number = typedValue;
                return true;
            case short typedValue:
                number = typedValue;
                return true;
            case int typedValue:
                number = typedValue;
                return true;
            case long typedValue:
                number = typedValue;
                return true;
            case float typedValue:
                number = typedValue;
                return true;
            case double typedValue:
                number = typedValue;
                return true;
            case decimal typedValue:
                number = (double)typedValue;
                return true;
            default:
                number = default;
                return false;
        }
    }
}
