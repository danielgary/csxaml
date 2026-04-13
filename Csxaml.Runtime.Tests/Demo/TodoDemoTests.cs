using GeneratedCsxaml;

namespace Csxaml.Runtime.Tests.Demo;

[TestClass]
public sealed class TodoDemoTests
{
    [TestMethod]
    public void TodoCard_SelectedState_UsesSelectedBackground()
    {
        var component = new TodoCardComponent();
        component.SetProps(new TodoCardProps("Ship milestone", true, true, () => { }, () => { }));

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();

        Assert.AreEqual(TodoColors.SelectedCardBackground, RuntimeTreeHelpers.GetProperty<ArgbColor>(tree, "Background"));
    }

    [TestMethod]
    public void TodoBoard_SelectingItemUpdatesEditorFields()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        Assert.AreEqual("Draft plan", GetEditorTextBox(firstEditor, 0));

        RuntimeTreeHelpers.GetEventHandler<Action>(GetSelectButton(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()), 1), "OnClick")();

        var secondEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        Assert.AreEqual("Wire runtime", GetEditorTextBox(secondEditor, 0));
        Assert.AreEqual("Reconcile the renderer and adapter flow.", GetEditorTextBox(secondEditor, 1));
    }

    [TestMethod]
    public void TodoBoard_TitleEditorUpdatesSelectedTodoTitle()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        RuntimeTreeHelpers.GetEventHandler<Action<string>>(GetTextBox(firstEditor, 0), "OnTextChanged")("Ship Milestone 4");

        var secondEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        Assert.AreEqual("Ship Milestone 4", GetEditorTextBox(secondEditor, 0));
        Assert.AreEqual("Ship Milestone 4", GetCardTitle(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()), 0));
    }

    [TestMethod]
    public void TodoBoard_TogglingOtherItemKeepsSelectedEditorContent()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action>(GetToggleButton(firstRoot, 1), "OnClick")();

        var secondRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        var secondEditor = GetEditor(secondRoot);

        Assert.AreEqual("Draft plan", GetEditorTextBox(secondEditor, 0));
        Assert.AreEqual("Sketch the interaction model for controlled inputs.", GetEditorTextBox(secondEditor, 1));
        Assert.AreEqual("Not Done", GetCardStatus(secondRoot, 1));
    }

    [TestMethod]
    public void TodoBoard_NotesEditorUpdatesSelectedTodoNotes()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        RuntimeTreeHelpers.GetEventHandler<Action<string>>(GetTextBox(firstEditor, 1), "OnTextChanged")("Add TextBox and CheckBox support.");

        var secondEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        Assert.AreEqual("Add TextBox and CheckBox support.", GetEditorTextBox(secondEditor, 1));
    }

    [TestMethod]
    public void TodoBoard_CheckBoxUpdatesSelectedTodoDoneState()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstEditor = GetEditor(RuntimeTreeHelpers.RootStackPanel(coordinator.Render()));
        RuntimeTreeHelpers.GetEventHandler<Action<bool>>(GetCheckBox(firstEditor), "OnCheckedChanged")(true);

        var secondRoot = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        var secondEditor = GetEditor(secondRoot);

        Assert.IsTrue(RuntimeTreeHelpers.GetProperty<bool?>(GetCheckBox(secondEditor), "IsChecked") ?? false);
        Assert.AreEqual("Done", GetCardStatus(secondRoot, 0));
    }

    private static NativeElementNode GetCard(NativeElementNode root, int index)
    {
        var listPanel = RuntimeTreeHelpers.GetChildElement(root, 0);
        return RuntimeTreeHelpers.GetChildElement(listPanel, index + 1);
    }

    private static string GetCardStatus(NativeElementNode root, int index)
    {
        var cardPanel = RuntimeTreeHelpers.GetChildElement(GetCard(root, index), 0);
        return RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(cardPanel, 1), "Text") ?? string.Empty;
    }

    private static string GetCardTitle(NativeElementNode root, int index)
    {
        var cardPanel = RuntimeTreeHelpers.GetChildElement(GetCard(root, index), 0);
        return RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(cardPanel, 0), "Text") ?? string.Empty;
    }

    private static NativeElementNode GetCheckBox(NativeElementNode editor)
    {
        return RuntimeTreeHelpers.GetChildElement(RuntimeTreeHelpers.GetChildElement(editor, 0), 4);
    }

    private static NativeElementNode GetEditor(NativeElementNode root)
    {
        return RuntimeTreeHelpers.GetChildElement(root, 1);
    }

    private static string GetEditorTextBox(NativeElementNode editor, int index)
    {
        return RuntimeTreeHelpers.GetProperty<string>(GetTextBox(editor, index), "Text") ?? string.Empty;
    }

    private static NativeElementNode GetSelectButton(NativeElementNode root, int index)
    {
        var cardPanel = RuntimeTreeHelpers.GetChildElement(GetCard(root, index), 0);
        return RuntimeTreeHelpers.GetChildElement(cardPanel, 2);
    }

    private static NativeElementNode GetToggleButton(NativeElementNode root, int index)
    {
        var cardPanel = RuntimeTreeHelpers.GetChildElement(GetCard(root, index), 0);
        return RuntimeTreeHelpers.GetChildElement(cardPanel, 3);
    }

    private static NativeElementNode GetTextBox(NativeElementNode editor, int index)
    {
        return RuntimeTreeHelpers.GetChildElement(RuntimeTreeHelpers.GetChildElement(editor, 0), index + 1);
    }
}
