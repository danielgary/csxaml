using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class PanelBackgroundPropertyApplicator
{
    public static void Apply(Panel panel, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Background", out var background))
        {
            panel.Background = BrushValueConverter.Convert(background);
            return;
        }

        panel.ClearValue(Panel.BackgroundProperty);
    }
}
