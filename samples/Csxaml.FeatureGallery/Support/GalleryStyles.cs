using Csxaml.Runtime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Samples.FeatureGallery;

public static class GalleryStyles
{
    public static DeferredStyle NavButton { get; } =
        new("FeatureGalleryNavButton", CreateNavButtonStyle);

    public static DeferredStyle PrimaryButton { get; } =
        new("FeatureGalleryPrimaryButton", CreatePrimaryButtonStyle);

    public static DeferredStyle SubtleButton { get; } =
        new("FeatureGallerySubtleButton", CreateSubtleButtonStyle);

    private static Style CreateNavButtonStyle()
    {
        var style = new Style { TargetType = typeof(Button) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(new ArgbColor(0, 255, 255, 255))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(GalleryColors.TextPrimary)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 8, 12, 8)));
        return style;
    }

    private static Style CreatePrimaryButtonStyle()
    {
        var style = new Style { TargetType = typeof(Button) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(GalleryColors.Accent)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(new ArgbColor(255, 255, 255, 255))));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14, 8, 14, 8)));
        return style;
    }

    private static Style CreateSubtleButtonStyle()
    {
        var style = new Style { TargetType = typeof(Button) };
        style.Setters.Add(new Setter(Control.BackgroundProperty, CreateBrush(GalleryColors.AccentSoft)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, CreateBrush(GalleryColors.TextPrimary)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 8, 12, 8)));
        return style;
    }

    private static SolidColorBrush CreateBrush(ArgbColor color)
    {
        return new SolidColorBrush(ColorHelper.FromArgb(color.A, color.R, color.G, color.B));
    }
}
