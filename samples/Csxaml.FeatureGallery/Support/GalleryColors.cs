using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace Csxaml.Samples.FeatureGallery;

public static class GalleryColors
{
    public static ArgbColor Accent { get; } = new(255, 0, 95, 184);

    public static ArgbColor AccentSoft { get; } = new(44, 0, 95, 184);

    public static ArgbColor AppBackground { get; } = new(0, 240, 244, 248);

    public static ArgbColor Border { get; } = new(92, 96, 112, 128);

    public static ArgbColor CodeBackground { get; } = new(238, 22, 27, 34);

    public static ArgbColor CodeText { get; } = new(255, 231, 237, 246);

    public static ArgbColor Panel { get; } = new(206, 255, 255, 255);

    public static ArgbColor PanelStrong { get; } = new(238, 255, 255, 255);

    public static ArgbColor Reveal { get; } = new(54, 0, 95, 184);

    public static ArgbColor TextMuted { get; } = new(255, 86, 96, 110);

    public static ArgbColor TextPrimary { get; } = new(255, 24, 28, 34);

    public static Thickness CardPadding { get; } = new(18);

    public static Thickness CompactPadding { get; } = new(10);
}
