namespace Csxaml.ControlMetadata;

internal static class GeneratedControlMetadata
{
    public static IReadOnlyList<ControlMetadata> All { get; } =
    [
        new ControlMetadata(
            "Border",
            "Microsoft.UI.Xaml.Controls.Border",
            "Microsoft.UI.Xaml.FrameworkElement",
            ControlChildKind.Single,
            [
                new PropertyMetadata("Background", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("BorderBrush", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("BorderThickness", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Padding", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
            ],
            [
            ]
        ),
        new ControlMetadata(
            "Button",
            "Microsoft.UI.Xaml.Controls.Button",
            "Microsoft.UI.Xaml.Controls.Primitives.ButtonBase",
            ControlChildKind.None,
            [
                new PropertyMetadata("Background", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("Content", "System.Object", true, false, false, true, ValueKindHint.Object),
                new PropertyMetadata("FontSize", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("Foreground", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
            ],
            [
                new EventMetadata("Click", "OnClick", "Microsoft.UI.Xaml.RoutedEventHandler", true, ValueKindHint.Unknown),
            ]
        ),
        new ControlMetadata(
            "StackPanel",
            "Microsoft.UI.Xaml.Controls.StackPanel",
            "Microsoft.UI.Xaml.Controls.Panel",
            ControlChildKind.Multiple,
            [
                new PropertyMetadata("Background", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("Orientation", "Microsoft.UI.Xaml.Controls.Orientation", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Spacing", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
            ]
        ),
        new ControlMetadata(
            "TextBlock",
            "Microsoft.UI.Xaml.Controls.TextBlock",
            "Microsoft.UI.Xaml.FrameworkElement",
            ControlChildKind.None,
            [
                new PropertyMetadata("FontSize", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("Foreground", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("Text", "System.String", true, false, false, true, ValueKindHint.String),
            ],
            [
            ]
        ),
    ];
}
