using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class StyleValueResolver
{
    public static Style? Resolve(object? value)
    {
        return value switch
        {
            null => null,
            Style style => style,
            DeferredStyle deferredStyle => deferredStyle.Resolve(),
            _ => throw new InvalidOperationException(
                $"Expected a style-compatible value but found '{value.GetType().Name}'.")
        };
    }
}
