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

    private static readonly IReadOnlyList<CuratedEventDefinition> CommonElementEvents =
    [
        EventArgsEvent("KeyDown", "OnKeyDown", "Microsoft.UI.Xaml.Input.KeyRoutedEventArgs"),
        EventArgsEvent("Loaded", "OnLoaded", "Microsoft.UI.Xaml.RoutedEventArgs"),
        EventArgsEvent("PointerCanceled", "OnPointerCanceled", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerCaptureLost", "OnPointerCaptureLost", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerEntered", "OnPointerEntered", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerExited", "OnPointerExited", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerMoved", "OnPointerMoved", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerPressed", "OnPointerPressed", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerReleased", "OnPointerReleased", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"),
        EventArgsEvent("PointerWheelChanged", "OnPointerWheelChanged", "Microsoft.UI.Xaml.Input.PointerRoutedEventArgs")
    ];

    public static IReadOnlyList<CuratedControlDefinition> Definitions { get; } =
    [
        new(
            typeof(Border),
            ControlChildKind.Single,
            WithCommonFrameworkElementProperties("Background", "BorderBrush", "BorderThickness", "Padding"),
            WithCommonEvents()),
        new(
            typeof(AutoSuggestBox),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("PlaceholderText", "Text"),
            WithCommonEvents(
                EventArgsEvent(
                    "QuerySubmitted",
                    "OnQuerySubmitted",
                    "Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs"),
                EventArgsEvent(
                    "SuggestionChosen",
                    "OnSuggestionChosen",
                    "Microsoft.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs"))),
        new(
            typeof(Button),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("Background", "Content", "FontSize", "Foreground"),
            WithCommonEvents(
                new CuratedEventDefinition(
                    ["Click"],
                    "Click",
                    "OnClick",
                    "System.Action",
                    ValueKindHint.Unknown,
                    EventBindingKind.Direct))),
        new(
            typeof(Canvas),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background"),
            WithCommonEvents()),
        new(
            typeof(CheckBox),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("Content", "IsChecked"),
            WithCommonEvents(
                new CuratedEventDefinition(
                    ["Checked", "Unchecked", "Indeterminate"],
                    null,
                    "OnCheckedChanged",
                    "System.Action<bool>",
                    ValueKindHint.Bool,
                    EventBindingKind.BoolValueChanged))),
        new(
            typeof(Frame),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties(),
            WithCommonEvents(
                EventArgsEvent("Navigated", "OnNavigated", "Microsoft.UI.Xaml.Navigation.NavigationEventArgs"),
                EventArgsEvent("Navigating", "OnNavigating", "Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs"),
                EventArgsEvent("NavigationFailed", "OnNavigationFailed", "Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs"),
                EventArgsEvent("NavigationStopped", "OnNavigationStopped", "Microsoft.UI.Xaml.Navigation.NavigationEventArgs"))),
        new(
            typeof(Grid),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background", "ColumnDefinitions", "RowDefinitions"),
            WithCommonEvents()),
        new(
            typeof(ListView),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("IsItemClickEnabled", "ItemsSource", "SelectionMode"),
            WithCommonEvents(
                EventArgsEvent("ItemClick", "OnItemClick", "Microsoft.UI.Xaml.Controls.ItemClickEventArgs"),
                EventArgsEvent("SelectionChanged", "OnSelectionChanged", "Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs"))),
        new(
            typeof(RelativePanel),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background"),
            WithCommonEvents()),
        new(
            typeof(ScrollViewer),
            ControlChildKind.Single,
            WithCommonFrameworkElementProperties(
                "HorizontalScrollBarVisibility",
                "HorizontalScrollMode",
                "VerticalScrollBarVisibility",
                "VerticalScrollMode"),
            WithCommonEvents()),
        new(
            typeof(Slider),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("Maximum", "Minimum", "Value"),
            WithCommonEvents(
                EventArgsEvent(
                    "ValueChanged",
                    "OnValueChanged",
                    "Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs"))),
        new(
            typeof(StackPanel),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background", "Orientation", "Spacing"),
            WithCommonEvents()),
        new(
            typeof(TextBlock),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("FontSize", "Foreground", "Text", "TextWrapping"),
            WithCommonEvents()),
        new(
            typeof(TextBox),
            ControlChildKind.None,
            WithCommonFrameworkElementProperties("AcceptsReturn", "MinHeight", "PlaceholderText", "Text", "TextWrapping"),
            WithCommonEvents(
                new CuratedEventDefinition(
                    ["TextChanged"],
                    "TextChanged",
                    "OnTextChanged",
                    "System.Action<string>",
                    ValueKindHint.String,
                    EventBindingKind.TextValueChanged))),
        new(
            typeof(VariableSizedWrapGrid),
            ControlChildKind.Multiple,
            WithCommonFrameworkElementProperties("Background"),
            WithCommonEvents())
    ];

    private static IReadOnlyList<string> WithCommonFrameworkElementProperties(params string[] propertyNames)
    {
        return propertyNames
            .Concat(CommonFrameworkElementPropertyNames)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<CuratedEventDefinition> WithCommonEvents(params CuratedEventDefinition[] eventDefinitions)
    {
        return CommonElementEvents
            .Concat(eventDefinitions)
            .ToArray();
    }

    private static CuratedEventDefinition EventArgsEvent(
        string clrEventName,
        string exposedName,
        string eventArgsTypeName)
    {
        return new CuratedEventDefinition(
            [clrEventName],
            clrEventName,
            exposedName,
            $"System.Action<{eventArgsTypeName}>",
            ValueKindHint.Unknown,
            EventBindingKind.EventArgs);
    }
}
