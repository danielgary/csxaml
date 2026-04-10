namespace Csxaml.Runtime.Tests;

internal static class RuntimeTreeHelpers
{
    public static string CardHeader(NativeElementNode card)
    {
        return GetProperty<string>(GetChildElement(card, 0), "Text") ?? string.Empty;
    }

    public static NativeElementNode ChildCard(NativeElementNode root, int index)
    {
        return GetChildElement(root, index);
    }

    public static void ClickIncrement(NativeElementNode card)
    {
        GetEventHandler<Action>(GetChildElement(card, card.Children.Count - 1), "OnClick")();
    }

    public static bool HasDoneBadge(NativeElementNode card)
    {
        return card.Children
            .OfType<NativeElementNode>()
            .Any(
                child => string.Equals(child.TagName, "TextBlock", StringComparison.Ordinal) &&
                    string.Equals(GetProperty<string>(child, "Text"), "Done", StringComparison.Ordinal));
    }

    public static NativeElementNode RootStackPanel(NativeNode root)
    {
        var element = (NativeElementNode)root;
        Assert.AreEqual("StackPanel", element.TagName);
        return element;
    }

    private static TDelegate GetEventHandler<TDelegate>(NativeElementNode node, string name)
        where TDelegate : Delegate
    {
        var handler = node.Events.Single(eventValue => eventValue.Name == name).Handler;
        return (TDelegate)handler;
    }

    private static NativeElementNode GetChildElement(NativeElementNode node, int index)
    {
        return (NativeElementNode)node.Children[index];
    }

    private static T? GetProperty<T>(NativeElementNode node, string name)
    {
        var property = node.Properties.Single(propertyValue => propertyValue.Name == name).Value;
        return property is null ? default : (T)property;
    }
}
