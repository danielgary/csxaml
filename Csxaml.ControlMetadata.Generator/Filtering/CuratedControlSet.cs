using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ControlMetadata.Generator;

internal static class CuratedControlSet
{
    private static readonly IReadOnlyList<string> CommonFrameworkElementPropertyNames =
    [
        "Height",
        "HorizontalAlignment",
        "Margin",
        "Style",
        "VerticalAlignment",
        "Width"
    ];

    public static IReadOnlyList<CuratedControlDefinition> Definitions { get; } =
    [
        new(
            typeof(Border),
            ControlChildKind.Single,
            WithCommonFrameworkElementProperties("Background", "BorderBrush", "BorderThickness", "Padding"),
            []),
        new(
            typeof(Button),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("Background", "Content", "FontSize", "Foreground"),
            [
                new CuratedEventDefinition(
                    ["Click"],
                    "Click",
                    "OnClick",
                    "System.Action",
                    ValueKindHint.Unknown,
                    EventBindingKind.Direct)
            ]),
        new(
            typeof(CheckBox),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("Content", "IsChecked"),
            [
                new CuratedEventDefinition(
                    ["Checked", "Unchecked", "Indeterminate"],
                    null,
                    "OnCheckedChanged",
                    "System.Action<bool>",
                    ValueKindHint.Bool,
                    EventBindingKind.BoolValueChanged)
            ]),
        new(
            typeof(Grid),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background", "ColumnDefinitions", "RowDefinitions"),
            []),
        new(
            typeof(ScrollViewer),
            ControlChildKind.Single,
            WithCommonFrameworkElementProperties(),
            []),
        new(
            typeof(StackPanel),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background", "Orientation", "Spacing"),
            []),
        new(
            typeof(TextBlock),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("FontSize", "Foreground", "Text"),
            []),
        new(
            typeof(TextBox),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("AcceptsReturn", "MinHeight", "PlaceholderText", "Text", "TextWrapping"),
            [
                new CuratedEventDefinition(
                    ["TextChanged"],
                    "TextChanged",
                    "OnTextChanged",
                    "System.Action<string>",
                    ValueKindHint.String,
                    EventBindingKind.TextValueChanged)
            ])
    ];

    private static IReadOnlyList<string> WithCommonFrameworkElementProperties(params string[] propertyNames)
    {
        return propertyNames
            .Concat(CommonFrameworkElementPropertyNames)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
