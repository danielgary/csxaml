using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.ExternalControls;

public sealed class StatusButton : Button
{
    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(
            nameof(BadgeText),
            typeof(string),
            typeof(StatusButton),
            new PropertyMetadata(string.Empty));

    public string? BadgeText
    {
        get => (string?)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    public void RaiseStatusChangedForTests(string status)
    {
        StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
    }
}

public sealed class StatusChangedEventArgs : EventArgs
{
    public StatusChangedEventArgs(string status)
    {
        Status = status;
    }

    public string Status { get; }
}
