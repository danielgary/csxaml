namespace Csxaml.ProjectSystem.Tests;

internal static class ProjectSystemTreeHelpers
{
    public static NativeElementNode? FindByAutomationName(NativeElementNode node, string automationName)
    {
        return FindByAttachedProperty(node, "AutomationProperties", "Name", automationName);
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
