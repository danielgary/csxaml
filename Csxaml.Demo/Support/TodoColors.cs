using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace Csxaml.Demo;

public static class TodoColors
{
    public static ArgbColor BoardForeground { get; } = new(255, 31, 41, 55);

    public static Thickness CardPadding { get; } = new(12);

    public static ArgbColor CardForeground { get; } = new(255, 255, 255, 255);

    public static ArgbColor EditorBackground { get; } = new(255, 238, 242, 255);

    public static ArgbColor EditorForeground { get; } = new(255, 31, 41, 55);

    public static ArgbColor EditorHintForeground { get; } = new(255, 90, 102, 117);

    public static ArgbColor DoneBackground { get; } = new(255, 46, 125, 50);

    public static ArgbColor DoneForeground { get; } = new(255, 255, 255, 255);

    public static ArgbColor NotDoneBackground { get; } = new(255, 198, 40, 40);

    public static ArgbColor NotDoneForeground { get; } = new(255, 255, 255, 255);

    public static ArgbColor SelectedCardBackground { get; } = new(255, 21, 101, 192);
}
