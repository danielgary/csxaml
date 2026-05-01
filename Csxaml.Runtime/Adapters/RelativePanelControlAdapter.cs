using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class RelativePanelControlAdapter : ControlAdapter<RelativePanel>
{
    public override string TagName => "RelativePanel";

    protected override void ApplyProperties(RelativePanel control, NativeElementNode node)
    {
        PanelBackgroundPropertyApplicator.Apply(control, node);
    }

    protected override void SetChildren(RelativePanel control, IReadOnlyList<UIElement> children)
    {
        UiElementCollectionPatcher.Update(control.Children, children);
    }
}
