namespace Csxaml.Testing;

internal static class NativeElementQuery
{
    public static NativeElementNode FindByAutomationId(NativeElementNode root, string automationId)
    {
        return TryFindByAutomationId(root, automationId) ??
            throw new InvalidOperationException($"Could not find a node with automation id '{automationId}'.");
    }

    public static NativeElementNode FindByAutomationName(NativeElementNode root, string automationName)
    {
        return TryFindByAutomationName(root, automationName) ??
            throw new InvalidOperationException($"Could not find a node with automation name '{automationName}'.");
    }

    public static NativeElementNode FindByText(NativeElementNode root, string text)
    {
        return TryFindByText(root, text) ??
            throw new InvalidOperationException($"Could not find a node with text '{text}'.");
    }

    public static NativeElementNode? TryFindByAutomationId(NativeElementNode root, string automationId)
    {
        return FindByPredicate(root, node => HasAttachedPropertyValue(node, "AutomationProperties", "AutomationId", automationId));
    }

    public static NativeElementNode? TryFindByAutomationName(NativeElementNode root, string automationName)
    {
        return FindByPredicate(root, node => HasAttachedPropertyValue(node, "AutomationProperties", "Name", automationName));
    }

    public static NativeElementNode? TryFindByText(NativeElementNode root, string text)
    {
        return FindByPredicate(root, node => HasPropertyValue(node, "Text", text) || HasPropertyValue(node, "Content", text));
    }

    private static NativeElementNode? FindByPredicate(
        NativeElementNode root,
        Func<NativeElementNode, bool> predicate)
    {
        if (predicate(root))
        {
            return root;
        }

        foreach (var child in root.Children.OfType<NativeElementNode>())
        {
            var match = FindByPredicate(child, predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static bool HasAttachedPropertyValue(
        NativeElementNode node,
        string ownerName,
        string propertyName,
        string expectedValue)
    {
        return string.Equals(
            node.AttachedProperties.SingleOrDefault(
                property =>
                    string.Equals(property.OwnerName, ownerName, StringComparison.Ordinal) &&
                    string.Equals(property.PropertyName, propertyName, StringComparison.Ordinal))?.Value as string,
            expectedValue,
            StringComparison.Ordinal);
    }

    private static bool HasPropertyValue(
        NativeElementNode node,
        string propertyName,
        string expectedValue)
    {
        return string.Equals(
            node.Properties.SingleOrDefault(
                property => string.Equals(property.Name, propertyName, StringComparison.Ordinal))?.Value as string,
            expectedValue,
            StringComparison.Ordinal);
    }
}
