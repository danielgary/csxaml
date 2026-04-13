using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class UiElementCollectionPatcher
{
    public static void Update(UIElementCollection current, IReadOnlyList<UIElement> next)
    {
        // Avoid Clear()/Add() so retained children stay attached when only sibling order changes.
        if (ReferenceEqualityList.Matches(current, next))
        {
            return;
        }

        RemoveDetachedChildren(current, next);
        InsertAndMoveChildren(current, next);
        TrimExtraChildren(current, next.Count);
    }

    private static void InsertAndMoveChildren(UIElementCollection current, IReadOnlyList<UIElement> next)
    {
        for (var index = 0; index < next.Count; index++)
        {
            var child = next[index];
            if (index < current.Count && ReferenceEquals(current[index], child))
            {
                continue;
            }

            MoveOrInsertChild(current, index, child);
        }
    }

    private static void MoveOrInsertChild(UIElementCollection current, int index, UIElement child)
    {
        var existingIndex = IndexOf(current, child);
        if (existingIndex >= 0)
        {
            current.RemoveAt(existingIndex);
        }

        if (index >= current.Count)
        {
            current.Add(child);
            return;
        }

        current.Insert(index, child);
    }

    private static void RemoveDetachedChildren(UIElementCollection current, IReadOnlyList<UIElement> next)
    {
        for (var index = current.Count - 1; index >= 0; index--)
        {
            if (Contains(next, current[index]))
            {
                continue;
            }

            current.RemoveAt(index);
        }
    }

    private static void TrimExtraChildren(UIElementCollection current, int desiredCount)
    {
        while (current.Count > desiredCount)
        {
            current.RemoveAt(current.Count - 1);
        }
    }

    private static bool Contains(IReadOnlyList<UIElement> children, UIElement child)
    {
        for (var index = 0; index < children.Count; index++)
        {
            if (ReferenceEquals(children[index], child))
            {
                return true;
            }
        }

        return false;
    }

    private static int IndexOf(UIElementCollection current, UIElement child)
    {
        for (var index = 0; index < current.Count; index++)
        {
            if (ReferenceEquals(current[index], child))
            {
                return index;
            }
        }

        return -1;
    }
}
