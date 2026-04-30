using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class VariableSizedWrapGridControlAdapter : ControlAdapter<VariableSizedWrapGrid>
{
    public override string TagName => "VariableSizedWrapGrid";

    protected override void ApplyProperties(VariableSizedWrapGrid control, NativeElementNode node)
    {
        PanelBackgroundPropertyApplicator.Apply(control, node);
    }

    protected override void SetChildren(VariableSizedWrapGrid control, IReadOnlyList<UIElement> children)
    {
        UiElementCollectionPatcher.Update(control.Children, children);
    }
}
