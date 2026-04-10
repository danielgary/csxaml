using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal static class BrushValueConverter
{
    public static Brush? Convert(object? value)
    {
        return value switch
        {
            null => null,
            ArgbColor color => new SolidColorBrush(ColorHelper.FromArgb(color.A, color.R, color.G, color.B)),
            Brush brush => brush,
            _ => throw new InvalidOperationException(
                $"Expected a brush-compatible value but found '{value.GetType().Name}'.")
        };
    }
}
