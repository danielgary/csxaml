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

    public static NativeElementNode RootGrid(NativeNode root)
    {
        var element = (NativeElementNode)root;
        Assert.AreEqual("Grid", element.TagName);
        return element;
    }

    public static TDelegate GetEventHandler<TDelegate>(NativeElementNode node, string name)
        where TDelegate : Delegate
    {
        var handler = node.Events.Single(eventValue => eventValue.Name == name).Handler;
        return (TDelegate)handler;
    }

    public static T? GetAttachedProperty<T>(
        NativeElementNode node,
        string ownerName,
        string propertyName)
    {
        var property = node.AttachedProperties.Single(
            attachedProperty =>
                attachedProperty.OwnerName == ownerName &&
                attachedProperty.PropertyName == propertyName);

        return property.Value is null ? default : (T)property.Value;
    }

    public static NativeElementNode GetChildElement(NativeElementNode node, int index)
    {
        return (NativeElementNode)node.Children[index];
    }

    public static NativeElementNode? FindByAutomationId(NativeElementNode node, string automationId)
    {
        return FindByAttachedProperty(
            node,
            "AutomationProperties",
            "AutomationId",
            automationId);
    }

    public static NativeElementNode? FindByAutomationName(NativeElementNode node, string automationName)
    {
        return FindByAttachedProperty(
            node,
            "AutomationProperties",
            "Name",
            automationName);
    }

    public static T? GetProperty<T>(NativeElementNode node, string name)
    {
        var property = node.Properties.Single(propertyValue => propertyValue.Name == name).Value;
        return property is null ? default : (T)property;
    }

    private static NativeElementNode? FindByAttachedProperty(
        NativeElementNode node,
        string ownerName,
        string propertyName,
        string expectedValue)
    {
        var property = node.AttachedProperties.SingleOrDefault(
            attachedProperty =>
                attachedProperty.OwnerName == ownerName &&
                attachedProperty.PropertyName == propertyName);
        if (property is not null &&
            string.Equals(property.Value as string, expectedValue, StringComparison.Ordinal))
        {
            return node;
        }

        foreach (var child in node.Children.OfType<NativeElementNode>())
        {
            var match = FindByAttachedProperty(child, ownerName, propertyName, expectedValue);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
