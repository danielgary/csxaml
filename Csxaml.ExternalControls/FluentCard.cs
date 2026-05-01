using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.ExternalControls;

[ContentProperty(Name = nameof(Child))]
public sealed class FluentCard : Grid
{
    public FluentCard()
    {
        Background = CreateRestBrush();
        BorderBrush = Brush(84, 96, 112, 128);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(8);
        Padding = new Thickness(18);
        HorizontalAlignment = HorizontalAlignment.Stretch;
    }

    public UIElement? Child
    {
        get => Children.Count == 0 ? null : Children[0];
        set
        {
            if (ReferenceEquals(Child, value))
            {
                return;
            }

            Children.Clear();
            if (value is not null)
            {
                Children.Add(value);
            }
        }
    }

    private static SolidColorBrush CreateRestBrush()
    {
        return Brush(222, 255, 255, 255);
    }

    private static SolidColorBrush Brush(byte alpha, byte red, byte green, byte blue)
    {
        return new SolidColorBrush(ColorHelper.FromArgb(alpha, red, green, blue));
    }
}
