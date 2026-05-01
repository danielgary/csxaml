using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Csxaml.ExternalControls;

[ContentProperty(Name = nameof(Example))]
public sealed class ControlExample : Button
{
    public UIElement? Example { get; set; }

    public UIElement? Options { get; set; }
}
