using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Rendering;

[TestClass]
[DoNotParallelize]
public sealed class WinUiNodeRendererTextBoxProjectionTests
{
    [TestMethod]
    public void Render_TextBox_RetainsFocusAcrossOrdinaryParentRerender()
    {
        WinUiTestEnvironment.RunInWindow(window =>
        {
            var renderer = new WinUiNodeRenderer();
            var firstRoot = (StackPanel)renderer.RenderProjectedRoot(CreateEditorHostNode("Draft plan"));

            window.Content = firstRoot;
            window.Activate();
            firstRoot.UpdateLayout();

            var firstEditor = (TextBox)firstRoot.Children[0];
            FocusOrInconclusive(firstEditor);

            var secondRoot = (StackPanel)renderer.RenderProjectedRoot(CreateEditorHostNode("Draft plan"));
            secondRoot.UpdateLayout();
            var secondEditor = (TextBox)secondRoot.Children[0];

            Assert.AreSame(firstRoot, secondRoot);
            Assert.AreSame(firstEditor, secondEditor);
            AssertFocused(secondEditor);
        });
    }

    [TestMethod]
    public void Render_TextBox_RestoresSelectionAcrossControlledUpdate()
    {
        WinUiTestEnvironment.RunInWindow(window =>
        {
            var renderer = new WinUiNodeRenderer();
            var firstRoot = (StackPanel)renderer.RenderProjectedRoot(CreateEditorHostNode("Draft plan"));

            window.Content = firstRoot;
            window.Activate();
            firstRoot.UpdateLayout();

            var firstEditor = (TextBox)firstRoot.Children[0];
            FocusOrInconclusive(firstEditor);
            firstEditor.SelectionStart = 2;
            firstEditor.SelectionLength = 4;

            var secondRoot = (StackPanel)renderer.RenderProjectedRoot(CreateEditorHostNode("Draft revised plan"));
            secondRoot.UpdateLayout();
            var secondEditor = (TextBox)secondRoot.Children[0];

            Assert.AreSame(firstRoot, secondRoot);
            Assert.AreSame(firstEditor, secondEditor);
            Assert.AreEqual("Draft revised plan", secondEditor.Text);
            Assert.AreEqual(2, secondEditor.SelectionStart);
            Assert.AreEqual(4, secondEditor.SelectionLength);
            AssertFocused(secondEditor);
        });
    }

    [TestMethod]
    public void Render_TextBox_RetainsFocusedEditorWhenSiblingKeyedListChanges()
    {
        WinUiTestEnvironment.RunInWindow(window =>
        {
            var renderer = new WinUiNodeRenderer();
            var firstRoot = (StackPanel)renderer.RenderProjectedRoot(
                CreateBoardNode(["todo-1", "todo-2"], "Draft plan"));

            window.Content = firstRoot;
            window.Activate();
            firstRoot.UpdateLayout();

            var firstEditor = (TextBox)firstRoot.Children[1];
            FocusOrInconclusive(firstEditor);
            firstEditor.SelectionStart = 1;
            firstEditor.SelectionLength = 5;

            var secondRoot = (StackPanel)renderer.RenderProjectedRoot(
                CreateBoardNode(["todo-2", "todo-1"], "Draft plan"));
            secondRoot.UpdateLayout();
            var secondEditor = (TextBox)secondRoot.Children[1];

            Assert.AreSame(firstRoot, secondRoot);
            Assert.AreSame(firstEditor, secondEditor);
            Assert.AreEqual(1, secondEditor.SelectionStart);
            Assert.AreEqual(5, secondEditor.SelectionLength);
            AssertFocused(secondEditor);
        });
    }

    private static void AssertFocused(Control control)
    {
        Assert.AreNotEqual(FocusState.Unfocused, control.FocusState);
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
                CreateTextBox("editor", editorText)
            ]);
    }

    private static NativeElementNode CreateEditorHostNode(string editorText)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                CreateTextBox("editor", editorText)
            ]);
    }

    private static NativeElementNode CreateTextBlock(string? key, string text)
    {
        return new NativeElementNode(
            "TextBlock",
            key,
            [new NativePropertyValue("Text", text, ValueKindHint.String)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static NativeElementNode CreateTextBox(string? key, string text)
    {
        return new NativeElementNode(
            "TextBox",
            key,
            [new NativePropertyValue("Text", text, ValueKindHint.String)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static void FocusOrInconclusive(Control control)
    {
        if (!control.Focus(FocusState.Programmatic))
        {
            Assert.Inconclusive("Unable to focus the projected WinUI control in this test environment.");
        }
    }
}
