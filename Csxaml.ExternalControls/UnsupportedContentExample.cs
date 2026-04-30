using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Csxaml.ExternalControls;

[ContentProperty(Name = nameof(UnsupportedContent))]
public sealed class UnsupportedContentExample : Button
{
    public int UnsupportedContent { get; set; }
}
