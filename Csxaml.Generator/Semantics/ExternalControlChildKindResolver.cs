using System.Reflection;

namespace Csxaml.Generator;

internal static class ExternalControlChildKindResolver
{
    public static ControlChildKind Resolve(Type controlType)
    {
        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.Panel"))
        {
            return ControlChildKind.Multiple;
        }

        if (HasChildrenCollection(controlType))
        {
            return ControlChildKind.Multiple;
        }

        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.ContentControl") ||
            InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.Border") ||
            InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.ScrollViewer") ||
            HasSingleChildProperty(controlType))
        {
            return ControlChildKind.Single;
        }

        return ControlChildKind.None;
    }

    private static bool HasChildrenCollection(Type controlType)
    {
        var property = controlType.GetProperty("Children", BindingFlags.Instance | BindingFlags.Public);
        return string.Equals(
            property?.PropertyType.FullName,
            "Microsoft.UI.Xaml.Controls.UIElementCollection",
            StringComparison.Ordinal);
    }

    private static bool HasSingleChildProperty(Type controlType)
    {
        return controlType.GetProperty("Child", BindingFlags.Instance | BindingFlags.Public) is not null ||
            controlType.GetProperty("Content", BindingFlags.Instance | BindingFlags.Public) is not null;
    }

    private static bool InheritsFrom(Type type, string baseTypeName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, baseTypeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
