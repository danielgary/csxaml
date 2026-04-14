namespace Csxaml.Generator;

internal static class ExternalControlValueKindResolver
{
    public static ValueKindHint Resolve(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (string.Equals(type.FullName, typeof(string).FullName, StringComparison.Ordinal))
        {
            return ValueKindHint.String;
        }

        if (string.Equals(type.FullName, typeof(bool).FullName, StringComparison.Ordinal))
        {
            return ValueKindHint.Bool;
        }

        if (string.Equals(type.FullName, typeof(int).FullName, StringComparison.Ordinal))
        {
            return ValueKindHint.Int;
        }

        if (string.Equals(type.FullName, typeof(double).FullName, StringComparison.Ordinal))
        {
            return ValueKindHint.Double;
        }

        if (type.IsEnum)
        {
            return ValueKindHint.Enum;
        }

        if (string.Equals(type.FullName, "Microsoft.UI.Xaml.Thickness", StringComparison.Ordinal))
        {
            return ValueKindHint.Thickness;
        }

        if (string.Equals(type.FullName, "Microsoft.UI.Xaml.Style", StringComparison.Ordinal))
        {
            return ValueKindHint.Style;
        }

        if (InheritsFrom(type, "Microsoft.UI.Xaml.Media.Brush"))
        {
            return ValueKindHint.Brush;
        }

        if (string.Equals(type.FullName, typeof(object).FullName, StringComparison.Ordinal))
        {
            return ValueKindHint.Object;
        }

        return ValueKindHint.Unknown;
    }

    private static bool InheritsFrom(Type type, string baseTypeName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, baseTypeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
