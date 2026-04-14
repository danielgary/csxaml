using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class FrameworkElementStylePropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Style", out var styleValue))
        {
            element.Style = StyleValueResolver.Resolve(styleValue);
            return;
        }

        element.ClearValue(FrameworkElement.StyleProperty);
    }
}
