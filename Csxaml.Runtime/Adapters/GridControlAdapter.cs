using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class GridControlAdapter : ControlAdapter<Grid>
{
    public override string TagName => "Grid";

    protected override void ApplyProperties(Grid control, NativeElementNode node)
    {
        ApplyBackground(control, node);
        ApplyColumnDefinitions(control, node);
        ApplyRowDefinitions(control, node);
    }

    protected override void SetChildren(Grid control, IReadOnlyList<UIElement> children)
    {
        UiElementCollectionPatcher.Update(control.Children, children);
    }

    private static void ApplyBackground(Grid control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Background", out var background))
        {
            control.Background = BrushValueConverter.Convert(background);
            return;
        }

        control.ClearValue(Panel.BackgroundProperty);
    }

    private static void ApplyColumnDefinitions(Grid control, NativeElementNode node)
    {
        if (!NativeElementReader.TryGetPropertyValue<object?>(node, "ColumnDefinitions", out var value))
        {
            GridDefinitionUpdater.UpdateColumns(control, Array.Empty<GridLength>());
            return;
        }

        GridDefinitionUpdater.UpdateColumns(control, ReadDefinitions(value, "ColumnDefinitions"));
    }

    private static void ApplyRowDefinitions(Grid control, NativeElementNode node)
    {
        if (!NativeElementReader.TryGetPropertyValue<object?>(node, "RowDefinitions", out var value))
        {
            GridDefinitionUpdater.UpdateRows(control, Array.Empty<GridLength>());
            return;
        }

        GridDefinitionUpdater.UpdateRows(control, ReadDefinitions(value, "RowDefinitions"));
    }

    private static IReadOnlyList<GridLength> ReadDefinitions(object? value, string propertyName)
    {
        return value switch
        {
            null => Array.Empty<GridLength>(),
            string text => GridLengthParser.Parse(text),
            GridLength[] lengths => lengths,
            IReadOnlyList<GridLength> lengths => lengths,
            IEnumerable<GridLength> lengths => lengths.ToArray(),
            _ => throw new InvalidOperationException(
                $"Grid property '{propertyName}' expected a string or GridLength sequence.")
        };
    }
}
