using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ExternalControls;

public sealed class StatusButton : Button
{
    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(
            nameof(BadgeText),
            typeof(string),
            typeof(StatusButton),
            new PropertyMetadata(string.Empty));

    public string? BadgeText
    {
        get => (string?)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }
}
