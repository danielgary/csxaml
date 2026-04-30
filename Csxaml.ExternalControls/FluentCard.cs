using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Csxaml.ExternalControls;

public sealed class FluentCard : ContentControl
{
    public FluentCard()
    {
        Background = CreateRestBrush();
        BorderBrush = Brush(84, 96, 112, 128);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(8);
        Padding = new Thickness(18);
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        TranslationTransition = new Vector3Transition { Duration = TimeSpan.FromMilliseconds(160) };
        ScaleTransition = new Vector3Transition { Duration = TimeSpan.FromMilliseconds(160) };

        PointerEntered += (_, _) => ApplyHoverState();
        PointerExited += (_, _) => ClearHoverState();
    }

    private void ApplyHoverState()
    {
        Background = CreateHoverBrush();
        Translation = new Vector3(0, -2, 0);
        Scale = new Vector3(1.01f, 1.01f, 1);
    }

    private void ClearHoverState()
    {
        Background = CreateRestBrush();
        Translation = Vector3.Zero;
        Scale = Vector3.One;
    }

    private static SolidColorBrush CreateRestBrush()
    {
        return Brush(222, 255, 255, 255);
    }

    private static SolidColorBrush CreateHoverBrush()
    {
        return Brush(242, 255, 255, 255);
    }

    private static SolidColorBrush Brush(byte alpha, byte red, byte green, byte blue)
    {
        return new SolidColorBrush(ColorHelper.FromArgb(alpha, red, green, blue));
    }
}
