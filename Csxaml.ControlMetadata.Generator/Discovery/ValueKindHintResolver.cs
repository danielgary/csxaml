using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.ControlMetadata.Generator;

internal static class ValueKindHintResolver
{
    public static ValueKindHint Resolve(Type type)
    {
        if (type == typeof(string))
        {
            return ValueKindHint.String;
        }

        if (type == typeof(bool))
        {
            return ValueKindHint.Bool;
        }

        if (type == typeof(int))
        {
            return ValueKindHint.Int;
        }

        if (type == typeof(double))
        {
            return ValueKindHint.Double;
        }

        if (type.IsEnum)
        {
            return ValueKindHint.Enum;
        }

        if (typeof(Brush).IsAssignableFrom(type))
        {
            return ValueKindHint.Brush;
        }

        if (type == typeof(Thickness))
        {
            return ValueKindHint.Thickness;
        }

        return ValueKindHint.Object;
    }
}
