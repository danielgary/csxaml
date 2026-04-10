namespace Csxaml.Runtime.Tests;

internal static class RuntimeTreeHelpers
{
    public static string CardHeader(StackPanelNode card)
    {
        return ((TextBlockNode)card.Children[0]).Text;
    }

    public static StackPanelNode ChildCard(StackPanelNode root, int index)
    {
        return (StackPanelNode)root.Children[index];
    }

    public static void ClickIncrement(StackPanelNode card)
    {
        ((ButtonNode)card.Children[^1]).OnClick();
    }

    public static bool HasDoneBadge(StackPanelNode card)
    {
        return card.Children
            .OfType<TextBlockNode>()
            .Any(textBlock => textBlock.Text == "Done");
    }

    public static StackPanelNode RootStackPanel(NativeNode root)
    {
        return (StackPanelNode)root;
    }
}
