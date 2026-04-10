using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator;

internal static class CuratedControlSet
{
    public static IReadOnlyList<CuratedControlDefinition> Definitions { get; } =
    [
        new(
            typeof(Border),
            ControlChildKind.Single,
            ["Background", "BorderBrush", "BorderThickness", "Padding"],
            new Dictionary<string, string>(StringComparer.Ordinal)),
        new(
            typeof(Button),
            ControlChildKind.None,
            ["Background", "Content", "FontSize", "Foreground"],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Click"] = "OnClick"
            }),
        new(
            typeof(StackPanel),
            ControlChildKind.Multiple,
            ["Background", "Orientation", "Spacing"],
            new Dictionary<string, string>(StringComparer.Ordinal)),
        new(
            typeof(TextBlock),
            ControlChildKind.None,
            ["FontSize", "Foreground", "Text"],
            new Dictionary<string, string>(StringComparer.Ordinal))
    ];
}
