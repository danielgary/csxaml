using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class ReferenceEqualityList
{
    public static bool Matches(UIElementCollection current, IReadOnlyList<UIElement> next)
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        for (var index = 0; index < next.Count; index++)
        {
            if (!ReferenceEquals(current[index], next[index]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool Matches<T>(IReadOnlyList<T> current, IReadOnlyList<T> next)
        where T : class
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        for (var index = 0; index < next.Count; index++)
        {
            if (!ReferenceEquals(current[index], next[index]))
            {
                return false;
            }
        }

        return true;
    }
}
