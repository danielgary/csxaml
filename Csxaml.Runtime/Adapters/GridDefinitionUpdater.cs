using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class GridDefinitionUpdater
{
    public static void UpdateColumns(Grid grid, IReadOnlyList<GridLength> definitions)
    {
        UpdateColumnDefinitions(grid.ColumnDefinitions, definitions);
    }

    public static void UpdateRows(Grid grid, IReadOnlyList<GridLength> definitions)
    {
        UpdateRowDefinitions(grid.RowDefinitions, definitions);
    }

    private static void UpdateColumnDefinitions(ColumnDefinitionCollection current, IReadOnlyList<GridLength> next)
    {
        if (ColumnDefinitionsMatch(current, next))
        {
            return;
        }

        current.Clear();
        foreach (var width in next)
        {
            current.Add(new ColumnDefinition { Width = width });
        }
    }

    private static void UpdateRowDefinitions(RowDefinitionCollection current, IReadOnlyList<GridLength> next)
    {
        if (RowDefinitionsMatch(current, next))
        {
            return;
        }

        current.Clear();
        foreach (var height in next)
        {
            current.Add(new RowDefinition { Height = height });
        }
    }

    private static bool ColumnDefinitionsMatch(
        ColumnDefinitionCollection current,
        IReadOnlyList<GridLength> next)
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        for (var index = 0; index < current.Count; index++)
        {
            if (!Equals(current[index].Width, next[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool RowDefinitionsMatch(
        RowDefinitionCollection current,
        IReadOnlyList<GridLength> next)
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        for (var index = 0; index < current.Count; index++)
        {
            if (!Equals(current[index].Height, next[index]))
            {
                return false;
            }
        }

        return true;
    }
}
