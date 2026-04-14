using Csxaml.ExternalControls;
using Csxaml.Runtime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Demo;

public static class TodoStyles
{
    public static DeferredStyle CardActionButton { get; } =
        new("TodoCardActionButtonStyle", CreateCardActionButtonStyle);

    public static DeferredStyle InteropInfoBar { get; } =
        new("TodoInteropInfoBarStyle", CreateInteropInfoBarStyle);

    public static DeferredStyle SelectionStatusButton { get; } =
        new("TodoSelectionStatusButtonStyle", CreateSelectionStatusButtonStyle);

    private static Style CreateCardActionButtonStyle()
    {
        var style = new Style { TargetType = typeof(Button) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(TodoColors.SelectedCardBackground)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(TodoColors.CardForeground)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 6, 12, 6)));
        return style;
    }

    private static Style CreateInteropInfoBarStyle()
    {
        var style = new Style { TargetType = typeof(InfoBar) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(TodoColors.EditorBackground)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(TodoColors.EditorForeground)));
        return style;
    }

    private static Style CreateSelectionStatusButtonStyle()
    {
        var style = new Style { TargetType = typeof(StatusButton) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(TodoColors.EditorBackground)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(TodoColors.EditorForeground)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14, 8, 14, 8)));
        return style;
    }

    private static SolidColorBrush CreateBrush(ArgbColor color)
    {
        return new SolidColorBrush(ColorHelper.FromArgb(color.A, color.R, color.G, color.B));
    }
}
