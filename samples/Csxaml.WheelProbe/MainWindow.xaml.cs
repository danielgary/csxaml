using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.Graphics;

namespace Csxaml.Samples.WheelProbe;

public sealed partial class MainWindow : Window
{
    private static readonly string[] Items = Enumerable.Range(1, 30)
        .Select(index => $"Wheel probe item {index:00}")
        .ToArray();

    public MainWindow()
    {
        InitializeComponent();
        ConfigureStartupWindow();
        PopulateLists();
        AttachHandledWheelDiagnostics();
    }

    private void ConfigureStartupWindow()
    {
        AppWindow.Title = "WinUI Wheel Probe";
        AppWindow.Resize(new SizeInt32(840, 780));
    }

    private void PopulateLists()
    {
        BareListView.ItemsSource = Items;
        BorderListView.ItemsSource = Items;
        FluentListView.ItemsSource = Items;
    }

    private void Root_Loaded(object sender, RoutedEventArgs args)
    {
        NativeWheelMessageProbe.Attach(
            Root.DispatcherQueue,
            status => NativeMessagesText.Text = status);
        UpdateBoundsStatus();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs args)
    {
        RouteText.Text = "Wheel route: waiting";
        NativeWheelMessageProbe.Reset();
        UpdateBoundsStatus();
    }

    private void Root_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Root", args);
    }

    private void BareListView_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Bare list", args);
    }

    private void BorderWrapper_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Border wrapper", args);
    }

    private void BorderListView_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Border list", args);
    }

    private void FluentContent_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Fluent content", args);
    }

    private void FluentListView_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        RecordWheelRoute("Fluent list", args);
    }

    private void RecordWheelRoute(string source, PointerRoutedEventArgs args)
    {
        var point = args.GetCurrentPoint(Root);
        var detail = $"{source} at root=({point.Position.X:0.##},{point.Position.Y:0.##}) delta={point.Properties.MouseWheelDelta}";

        if (RouteText.Text == "Wheel route: waiting")
        {
            RouteText.Text = $"Wheel route: {detail}";
            return;
        }

        if (!RouteText.Text.Contains(source, StringComparison.Ordinal))
        {
            RouteText.Text = $"{RouteText.Text} > {detail}";
        }
    }

    private void AttachHandledWheelDiagnostics()
    {
        AddHandledWheelDiagnostic(Root, "Root handled");
        AddHandledWheelDiagnostic(BareListView, "Bare list handled");
        AddHandledWheelDiagnostic(BorderWrapper, "Border wrapper handled");
        AddHandledWheelDiagnostic(BorderListView, "Border list handled");
        AddHandledWheelDiagnostic(FluentContent, "Fluent content handled");
        AddHandledWheelDiagnostic(FluentListView, "Fluent list handled");
    }

    private void AddHandledWheelDiagnostic(UIElement element, string source)
    {
        element.AddHandler(
            UIElement.PointerWheelChangedEvent,
            new PointerEventHandler((_, args) => RecordWheelRoute(source, args)),
            handledEventsToo: true);
    }

    private void UpdateBoundsStatus()
    {
        BoundsText.Text = string.Join(
            "; ",
            "Bounds:",
            $"root={FormatSize(Root)}",
            $"bare={FormatBounds(BareListView)}",
            $"border={FormatBounds(BorderListView)}",
            $"fluent={FormatBounds(FluentListView)}");
    }

    private string FormatBounds(FrameworkElement element)
    {
        var bounds = element
            .TransformToVisual(Root)
            .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

        return $"({bounds.X:0.##},{bounds.Y:0.##},{bounds.Width:0.##},{bounds.Height:0.##})";
    }

    private static string FormatSize(FrameworkElement element)
    {
        return $"({element.ActualWidth:0.##},{element.ActualHeight:0.##})";
    }
}
