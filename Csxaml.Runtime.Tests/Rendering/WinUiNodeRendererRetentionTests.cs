namespace Csxaml.Runtime.Tests.Rendering;

[TestClass]
public sealed class WinUiNodeRendererRetentionTests
{
    [TestMethod]
    public void Render_ReusesKeyedChildrenAcrossReorder()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("StackPanel"),
            new FakeControlAdapter("TextBlock", supportsChildren: false));

        var firstRoot = (FakeElement)renderer.RenderProjectedRoot(CreateKeyedList("alpha", "beta"));
        var firstAlpha = firstRoot.Children[0];
        var firstBeta = firstRoot.Children[1];

        var secondRoot = (FakeElement)renderer.RenderProjectedRoot(CreateKeyedList("beta", "alpha"));

        Assert.AreSame(firstRoot, secondRoot);
        Assert.AreSame(firstBeta, secondRoot.Children[0]);
        Assert.AreSame(firstAlpha, secondRoot.Children[1]);
    }

    [TestMethod]
    public void Render_ReplacesElement_WhenKeyChanges()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));

        var firstTextBox = (FakeElement)renderer.RenderProjectedRoot(
            CreateTextBox("todo-1", "Draft plan"));

        var secondTextBox = (FakeElement)renderer.RenderProjectedRoot(
            CreateTextBox("todo-2", "Draft plan"));

        Assert.AreNotSame(firstTextBox, secondTextBox);
    }

    [TestMethod]
    public void Render_RejectsDuplicateSiblingKeys()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("StackPanel"),
            new FakeControlAdapter("TextBlock", supportsChildren: false));

        InvalidOperationException? exception = null;

        try
        {
            renderer.RenderProjectedRoot(
                new NativeElementNode(
                    "StackPanel",
                    null,
                    Array.Empty<NativePropertyValue>(),
                    Array.Empty<NativeEventValue>(),
                    [
                        CreateTextBlock("todo-1", "Draft"),
                        CreateTextBlock("todo-1", "Duplicate")
                    ]));
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "todo-1");
    }

    [TestMethod]
    public void Render_RetainsEditorTextBox_WhenSiblingKeyedListChanges()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("StackPanel"),
            new FakeControlAdapter("TextBlock", supportsChildren: false),
            new FakeControlAdapter("TextBox", supportsChildren: false));

        var firstRoot = (FakeElement)renderer.RenderProjectedRoot(CreateBoardNode(["todo-1", "todo-2"], "Draft plan"));
        var firstEditor = firstRoot.Children[1];

        var secondRoot = (FakeElement)renderer.RenderProjectedRoot(CreateBoardNode(["todo-2", "todo-1"], "Draft plan"));
        var secondEditor = secondRoot.Children[1];

        Assert.AreSame(firstRoot, secondRoot);
        Assert.AreSame(firstEditor, secondEditor);
        Assert.AreEqual("Draft plan", secondEditor.Properties["Text"]);
    }

    private static NativeElementNode CreateBoardNode(string[] itemKeys, string editorText)
    {
        var listChildren = new List<Node>(itemKeys.Length);
        foreach (var itemKey in itemKeys)
        {
            listChildren.Add(CreateTextBlock(itemKey, itemKey));
        }

        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "StackPanel",
                    null,
                    Array.Empty<NativePropertyValue>(),
                    Array.Empty<NativeEventValue>(),
                    listChildren),
                CreateTextBox(null, editorText)
            ]);
    }

    private static NativeElementNode CreateKeyedList(string firstKey, string secondKey)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                CreateTextBlock(firstKey, firstKey),
                CreateTextBlock(secondKey, secondKey)
            ]);
    }

    private static NativeElementNode CreateTextBlock(string? key, string text)
    {
        return new NativeElementNode(
            "TextBlock",
            key,
            [new NativePropertyValue("Text", text)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static NativeElementNode CreateTextBox(string? key, string text)
    {
        return new NativeElementNode(
            "TextBox",
            key,
            [new NativePropertyValue("Text", text, global::Csxaml.ControlMetadata.ValueKindHint.String)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static WinUiNodeRenderer CreateRenderer(params INativeControlAdapter[] adapters)
    {
        return new WinUiNodeRenderer(new ControlAdapterRegistry(adapters));
    }
}
