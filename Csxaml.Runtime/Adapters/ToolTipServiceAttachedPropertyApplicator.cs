using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class ToolTipServiceAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        if (!string.Equals(property.PropertyName, "ToolTip", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Unsupported ToolTipService attached property '{property.PropertyName}'.");
        }

        ToolTipService.SetToolTip(element, property.Value);
    }

    public static void Clear(FrameworkElement element)
    {
        element.ClearValue(ToolTipService.ToolTipProperty);
    }
}
