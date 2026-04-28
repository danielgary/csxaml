namespace Csxaml.Benchmarks;

internal static class WinUiProjectionScenarioNodes
{
    public static NativeElementNode CreateEditorHostNode(string editorKey, string editorText)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                CreateTextBox(editorKey, editorText)
            ]);
    }

    public static NativeElementNode CreateBoardNode(
        IReadOnlyList<string> itemKeys,
        string editorKey,
        string editorText)
    {
        var listChildren = new Node[itemKeys.Count];
        for (var i = 0; i < itemKeys.Count; i++)
        {
            listChildren[i] = CreateTextBlock(itemKeys[i], itemKeys[i]);
        }

        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "StackPanel",
                    "sidebar",
                    Array.Empty<NativePropertyValue>(),
                    Array.Empty<NativeEventValue>(),
                    listChildren),
                CreateTextBox(editorKey, editorText)
            ]);
    }

    private static NativeElementNode CreateTextBlock(string key, string text)
    {
        return new NativeElementNode(
            "TextBlock",
            key,
            [new NativePropertyValue("Text", text, ValueKindHint.String)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static NativeElementNode CreateTextBox(string key, string text)
    {
        return new NativeElementNode(
            "TextBox",
            key,
            [new NativePropertyValue("Text", text, ValueKindHint.String)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
