using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Csxaml.ExternalControls;

[ContentProperty(Name = nameof(Example))]
public class InheritedControlExampleBase : Button
{
    public UIElement? Example { get; set; }
}

public sealed class InheritedControlExample : InheritedControlExampleBase
{
}
