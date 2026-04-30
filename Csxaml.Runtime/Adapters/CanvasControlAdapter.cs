using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class CanvasControlAdapter : ControlAdapter<Canvas>
{
    public override string TagName => "Canvas";

    protected override void ApplyProperties(Canvas control, NativeElementNode node)
    {
        PanelBackgroundPropertyApplicator.Apply(control, node);
    }

    protected override void SetChildren(Canvas control, IReadOnlyList<UIElement> children)
    {
        UiElementCollectionPatcher.Update(control.Children, children);
    }
}
