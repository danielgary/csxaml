using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal readonly record struct TextSelectionRange(int Start, int Length)
{
    public static TextSelectionRange Capture(TextBox control)
    {
        return new TextSelectionRange(control.SelectionStart, control.SelectionLength);
    }

    public TextSelectionRange Clamp(int textLength)
    {
        var start = Math.Clamp(Start, 0, textLength);
        var length = Math.Clamp(Length, 0, textLength - start);
        return new TextSelectionRange(start, length);
    }

    public void Restore(TextBox control, string text)
    {
        var clamped = Clamp(text.Length);
        control.Select(clamped.Start, clamped.Length);
    }
}
