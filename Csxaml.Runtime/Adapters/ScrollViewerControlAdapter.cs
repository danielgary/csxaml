using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class ScrollViewerControlAdapter : ControlAdapter<ScrollViewer>
{
    public override string TagName => "ScrollViewer";

    protected override void ApplyProperties(ScrollViewer control, NativeElementNode node)
    {
    }

    protected override void SetChildren(ScrollViewer control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 1)
        {
            throw new InvalidOperationException("ScrollViewer supports only one child.");
        }

        var nextChild = children.Count == 0 ? null : children[0];
        if (ReferenceEquals(control.Content, nextChild))
        {
            return;
        }

        control.Content = nextChild;
    }
}
