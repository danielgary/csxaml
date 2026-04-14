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
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Padding", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
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
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
                new EventMetadata("Click", "OnClick", "System.Action", true, ValueKindHint.Unknown, EventBindingKind.Direct),
            ]
        ),
        new ControlMetadata(
            "CheckBox",
            "Microsoft.UI.Xaml.Controls.CheckBox",
            "Microsoft.UI.Xaml.Controls.Primitives.ToggleButton",
            ControlChildKind.None,
            [
                new PropertyMetadata("Content", "System.Object", true, false, false, true, ValueKindHint.Object),
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("IsChecked", "System.Nullable`1[[System.Boolean, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]", true, false, false, true, ValueKindHint.Bool),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
                new EventMetadata(null, "OnCheckedChanged", "System.Action<bool>", true, ValueKindHint.Bool, EventBindingKind.BoolValueChanged),
            ]
        ),
        new ControlMetadata(
            "Grid",
            "Microsoft.UI.Xaml.Controls.Grid",
            "Microsoft.UI.Xaml.Controls.Panel",
            ControlChildKind.Multiple,
            [
                new PropertyMetadata("Background", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("ColumnDefinitions", "Microsoft.UI.Xaml.Controls.ColumnDefinitionCollection", false, false, false, true, ValueKindHint.Object),
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("RowDefinitions", "Microsoft.UI.Xaml.Controls.RowDefinitionCollection", false, false, false, true, ValueKindHint.Object),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
            ]
        ),
        new ControlMetadata(
            "ScrollViewer",
            "Microsoft.UI.Xaml.Controls.ScrollViewer",
            "Microsoft.UI.Xaml.Controls.ContentControl",
            ControlChildKind.Single,
            [
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
            ]
        ),
        new ControlMetadata(
            "StackPanel",
            "Microsoft.UI.Xaml.Controls.StackPanel",
            "Microsoft.UI.Xaml.Controls.Panel",
            ControlChildKind.Multiple,
            [
                new PropertyMetadata("Background", "Microsoft.UI.Xaml.Media.Brush", true, false, false, true, ValueKindHint.Brush),
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Orientation", "Microsoft.UI.Xaml.Controls.Orientation", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Spacing", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
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
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("Text", "System.String", true, false, false, true, ValueKindHint.String),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
            ]
        ),
        new ControlMetadata(
            "TextBox",
            "Microsoft.UI.Xaml.Controls.TextBox",
            "Microsoft.UI.Xaml.Controls.Control",
            ControlChildKind.None,
            [
                new PropertyMetadata("AcceptsReturn", "System.Boolean", true, false, false, true, ValueKindHint.Bool),
                new PropertyMetadata("Height", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("HorizontalAlignment", "Microsoft.UI.Xaml.HorizontalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Margin", "Microsoft.UI.Xaml.Thickness", true, false, false, true, ValueKindHint.Thickness),
                new PropertyMetadata("MinHeight", "System.Double", true, false, false, true, ValueKindHint.Double),
                new PropertyMetadata("PlaceholderText", "System.String", true, false, false, true, ValueKindHint.String),
                new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, false, false, true, ValueKindHint.Style),
                new PropertyMetadata("Text", "System.String", true, false, false, true, ValueKindHint.String),
                new PropertyMetadata("TextWrapping", "Microsoft.UI.Xaml.TextWrapping", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("VerticalAlignment", "Microsoft.UI.Xaml.VerticalAlignment", true, false, false, true, ValueKindHint.Enum),
                new PropertyMetadata("Width", "System.Double", true, false, false, true, ValueKindHint.Double),
            ],
            [
                new EventMetadata("TextChanged", "OnTextChanged", "System.Action<string>", true, ValueKindHint.String, EventBindingKind.TextValueChanged),
            ]
        ),
    ];
}
