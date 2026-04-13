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
            []),
        new(
            typeof(Button),
            ControlChildKind.None,
            ["Background", "Content", "FontSize", "Foreground"],
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
            ["Content", "IsChecked"],
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
            typeof(StackPanel),
            ControlChildKind.Multiple,
            ["Background", "Orientation", "Spacing"],
            []),
        new(
            typeof(TextBlock),
            ControlChildKind.None,
            ["FontSize", "Foreground", "Text"],
            []),
        new(
            typeof(TextBox),
            ControlChildKind.None,
            ["AcceptsReturn", "MinHeight", "PlaceholderText", "Text", "TextWrapping", "Width"],
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
}
