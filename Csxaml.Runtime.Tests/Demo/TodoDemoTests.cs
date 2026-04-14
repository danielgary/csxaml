using Csxaml.ExternalControls;
using Csxaml.Demo;
using Microsoft.UI.Xaml.Controls;

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
    public void TodoBoard_UsesGridLayoutAndSemanticHooks()
    {
        var component = new TodoBoardComponent();

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();

        Assert.AreEqual("Grid", tree.TagName);
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationName(tree, "Todo Board Title"));
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationName(tree, "Selection Status Button"));
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationName(tree, "External WinUI Proof"));
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationName(tree, "Todo List"));
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationName(tree, "Task Editor"));
        Assert.IsNotNull(RuntimeTreeHelpers.FindByAutomationId(tree, "SelectedTodoTitle"));
        Assert.AreEqual(
            1,
            RuntimeTreeHelpers.GetAttachedProperty<int>(
                RuntimeTreeHelpers.FindByAutomationName(tree, "Task Editor")!,
                "Grid",
                "Column"));
    }

    [TestMethod]
    public void TodoBoard_ExternalInteropControls_AppearWithResolvedRuntimeKeys()
    {
        var component = new TodoBoardComponent();

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();
        var statusButton = RuntimeTreeHelpers.FindByAutomationName(tree, "Selection Status Button");
        var infoBar = RuntimeTreeHelpers.FindByAutomationName(tree, "External WinUI Proof");

        Assert.IsNotNull(statusButton);
        Assert.IsNotNull(infoBar);
        Assert.AreEqual(typeof(StatusButton).FullName, statusButton!.TagName);
        Assert.AreEqual(typeof(InfoBar).FullName, infoBar!.TagName);
        Assert.AreEqual("todo-1", RuntimeTreeHelpers.GetProperty<string>(statusButton, "BadgeText"));
    }

    [TestMethod]
    public void TodoBoard_UsesDeferredStyleObjectsOnNativeControls()
    {
        var component = new TodoBoardComponent();

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();
        var statusButton = RuntimeTreeHelpers.FindByAutomationName(tree, "Selection Status Button");
        var infoBar = RuntimeTreeHelpers.FindByAutomationName(tree, "External WinUI Proof");

        AssertDeferredStyle(statusButton!);
        AssertDeferredStyle(infoBar!);
        AssertDeferredStyle(GetSelectButton(tree, 0));
        AssertDeferredStyle(GetToggleButton(tree, 0));
    }

    [TestMethod]
    public void TodoBoard_SelectingItemUpdatesEditorFields()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Draft plan", GetTitleEditorText(firstRoot));

        RuntimeTreeHelpers.GetEventHandler<Action>(GetSelectButton(firstRoot, 1), "OnClick")();

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Wire runtime", GetTitleEditorText(secondRoot));
        Assert.AreEqual("Reconcile the renderer and adapter flow.", GetNotesEditorText(secondRoot));
    }

    [TestMethod]
    public void TodoBoard_StatusButtonResetsSelectionToFirstItem()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action>(GetSelectButton(firstRoot, 1), "OnClick")();

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Wire runtime", GetTitleEditorText(secondRoot));

        RuntimeTreeHelpers.GetEventHandler<Action>(GetSelectionStatusButton(secondRoot), "OnClick")();

        var thirdRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Draft plan", GetTitleEditorText(thirdRoot));
    }

    [TestMethod]
    public void TodoBoard_TitleEditorUpdatesSelectedTodoTitle()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action<string>>(GetTitleEditor(firstRoot), "OnTextChanged")("Ship Milestone 6");

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Ship Milestone 6", GetTitleEditorText(secondRoot));
        Assert.AreEqual("Ship Milestone 6", GetCardTitle(secondRoot, 0));
    }

    [TestMethod]
    public void TodoBoard_TogglingOtherItemKeepsSelectedEditorContent()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action>(GetToggleButton(firstRoot, 1), "OnClick")();

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());

        Assert.AreEqual("Draft plan", GetTitleEditorText(secondRoot));
        Assert.AreEqual("Sketch the interaction model for controlled inputs.", GetNotesEditorText(secondRoot));
        Assert.AreEqual("Not Done", GetCardStatus(secondRoot, 1));
    }

    [TestMethod]
    public void TodoBoard_NotesEditorUpdatesSelectedTodoNotes()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action<string>>(GetNotesEditor(firstRoot), "OnTextChanged")("Add Grid and ScrollViewer support.");

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Add Grid and ScrollViewer support.", GetNotesEditorText(secondRoot));
    }

    [TestMethod]
    public void TodoBoard_CheckBoxUpdatesSelectedTodoDoneState()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action<bool>>(GetDoneCheckBox(firstRoot), "OnCheckedChanged")(true);

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());

        Assert.IsTrue(RuntimeTreeHelpers.GetProperty<bool?>(GetDoneCheckBox(secondRoot), "IsChecked") ?? false);
        Assert.AreEqual("Done", GetCardStatus(secondRoot, 0));
    }

    private static NativeElementNode GetCard(NativeElementNode root, int index)
    {
        var listPanel = RuntimeTreeHelpers.GetChildElement(
            RuntimeTreeHelpers.FindByAutomationName(root, "Todo List")!,
            0);
        return RuntimeTreeHelpers.GetChildElement(listPanel, index);
    }

    private static void AssertDeferredStyle(NativeElementNode node)
    {
        var style = RuntimeTreeHelpers.GetProperty<object>(node, "Style");
        Assert.IsNotNull(style);
        Assert.IsInstanceOfType(style, typeof(DeferredStyle));
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

    private static NativeElementNode GetDoneCheckBox(NativeElementNode root)
    {
        return RuntimeTreeHelpers.FindByAutomationId(root, "SelectedTodoDone")!;
    }

    private static NativeElementNode GetNotesEditor(NativeElementNode root)
    {
        return RuntimeTreeHelpers.FindByAutomationId(root, "SelectedTodoNotes")!;
    }

    private static NativeElementNode GetSelectionStatusButton(NativeElementNode root)
    {
        return RuntimeTreeHelpers.FindByAutomationName(root, "Selection Status Button")!;
    }

    private static string GetNotesEditorText(NativeElementNode root)
    {
        return RuntimeTreeHelpers.GetProperty<string>(GetNotesEditor(root), "Text") ?? string.Empty;
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

    private static NativeElementNode GetTitleEditor(NativeElementNode root)
    {
        return RuntimeTreeHelpers.FindByAutomationId(root, "SelectedTodoTitle")!;
    }

    private static string GetTitleEditorText(NativeElementNode root)
    {
        return RuntimeTreeHelpers.GetProperty<string>(GetTitleEditor(root), "Text") ?? string.Empty;
    }
}
