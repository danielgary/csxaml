using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

public sealed class WinUiNodeRenderer
{
    public UIElement Render(NativeNode node)
    {
        return node switch
        {
            ButtonNode buttonNode => RenderButton(buttonNode),
            StackPanelNode stackPanelNode => RenderStackPanel(stackPanelNode),
            TextBlockNode textBlockNode => new TextBlock
            {
                Text = textBlockNode.Text
            },
            _ => throw new NotSupportedException(
                $"Unsupported native node type '{node.GetType().Name}'.")
        };
    }

    private static Button RenderButton(ButtonNode buttonNode)
    {
        var button = new Button
        {
            Content = buttonNode.Content
        };

        button.Click += (_, _) => buttonNode.OnClick();
        return button;
    }

    private StackPanel RenderStackPanel(StackPanelNode stackPanelNode)
    {
        var panel = new StackPanel();
        foreach (var child in stackPanelNode.Children)
        {
            panel.Children.Add(Render((NativeNode)child));
        }

        return panel;
    }
}
