using Csxaml.ExternalControls;
using Csxaml.Samples.TodoApp;
using Csxaml.Testing;
using Microsoft.Extensions.DependencyInjection;

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
        using var render = CsxamlTestHost.Render<TodoBoardComponent>(CreateServices());
        var tree = render.Root;

        Assert.AreEqual("Grid", tree.TagName);
        Assert.IsNotNull(render.FindByAutomationName("Todo Board Title"));
        Assert.IsNotNull(render.FindByAutomationName("Selection Status Button"));
        Assert.IsNotNull(render.FindByAutomationName("Generated App Proof"));
        Assert.IsNotNull(render.FindByAutomationName("Todo List"));
        Assert.IsNotNull(render.FindByAutomationName("Task Editor"));
        Assert.IsNotNull(render.FindByAutomationId("SelectedTodoTitle"));
        Assert.IsNotNull(render.FindByText("Generated app mode"));
        Assert.IsNotNull(render.FindByText("Draft plan"));
        Assert.AreEqual(
            1,
            RuntimeTreeHelpers.GetAttachedProperty<int>(
                render.FindByAutomationName("Task Editor"),
                "Grid",
                "Column"));
    }

    [TestMethod]
    public void TodoBoard_ExternalInteropControls_AppearWithResolvedRuntimeKeys()
    {
        var component = new TodoBoardComponent();

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component, CreateServices()).Render();
        var statusButton = RuntimeTreeHelpers.FindByAutomationName(tree, "Selection Status Button");
        var generatedProof = RuntimeTreeHelpers.FindByAutomationName(tree, "Generated App Proof");

        Assert.IsNotNull(statusButton);
        Assert.IsNotNull(generatedProof);
        Assert.AreEqual(typeof(StatusButton).FullName, statusButton!.TagName);
        Assert.AreEqual("Border", generatedProof!.TagName);
        Assert.AreEqual("todo-1", RuntimeTreeHelpers.GetProperty<string>(statusButton, "BadgeText"));
    }

    [TestMethod]
    public void TodoBoard_UsesDeferredStyleObjectsOnNativeControls()
    {
        var component = new TodoBoardComponent();

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component, CreateServices()).Render();
        var statusButton = RuntimeTreeHelpers.FindByAutomationName(tree, "Selection Status Button");

        AssertDeferredStyle(statusButton!);
        AssertDeferredStyle(GetSelectButton(tree, 0));
        AssertDeferredStyle(GetToggleButton(tree, 0));
    }

    [TestMethod]
    public void TodoBoard_SelectingItemUpdatesEditorFields()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component, CreateServices());

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
        var coordinator = new ComponentTreeCoordinator(component, CreateServices());

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
        var service = new RecordingTodoService(new InMemoryTodoService().LoadItems());
        using var render = CsxamlTestHost.Render<TodoBoardComponent>(CreateServices(service));

        render.EnterText(render.FindByAutomationId("SelectedTodoTitle"), "Ship Milestone 6");

        Assert.AreEqual("Ship Milestone 6", GetTitleEditorText(RuntimeTreeHelpers.RootGrid(render.Root)));
        Assert.AreEqual("Ship Milestone 6", GetCardTitle(RuntimeTreeHelpers.RootGrid(render.Root), 0));
        Assert.AreEqual("Ship Milestone 6", service.Snapshot()[0].Title);
    }

    [TestMethod]
    public void TodoBoard_TogglingOtherItemKeepsSelectedEditorContent()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component, CreateServices());

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
        var coordinator = new ComponentTreeCoordinator(component, CreateServices());

        var firstRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        RuntimeTreeHelpers.GetEventHandler<Action<string>>(GetNotesEditor(firstRoot), "OnTextChanged")("Add Grid and ScrollViewer support.");

        var secondRoot = RuntimeTreeHelpers.RootGrid(coordinator.Render());
        Assert.AreEqual("Add Grid and ScrollViewer support.", GetNotesEditorText(secondRoot));
    }

    [TestMethod]
    public void TodoBoard_CheckBoxUpdatesSelectedTodoDoneState()
    {
        using var render = CsxamlTestHost.Render<TodoBoardComponent>(CreateServices());

        render.SetChecked(render.FindByAutomationId("SelectedTodoDone"), true);

        var root = RuntimeTreeHelpers.RootGrid(render.Root);
        Assert.IsTrue(RuntimeTreeHelpers.GetProperty<bool?>(GetDoneCheckBox(root), "IsChecked") ?? false);
        Assert.AreEqual("Done", GetCardStatus(root, 0));
    }

    [TestMethod]
    public void TodoBoard_LoadsInitialItemsFromInjectedService()
    {
        var service = new RecordingTodoService(
            new[]
            {
                new TodoItemModel("custom-1", "Review injected seed", "Loaded from the test service.", false),
                new TodoItemModel("custom-2", "Confirm persistence", "Wire edits back through the service.", true)
            });
        using var render = CsxamlTestHost.Render<TodoBoardComponent>(CreateServices(service));

        var root = RuntimeTreeHelpers.RootGrid(render.Root);
        var statusButton = GetSelectionStatusButton(root);

        Assert.IsNotNull(render.FindByText("Review injected seed"));
        Assert.AreEqual("custom-1", RuntimeTreeHelpers.GetProperty<string>(statusButton, "BadgeText"));
        Assert.AreEqual("Review injected seed", GetTitleEditorText(root));
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

    private static IServiceProvider CreateServices(ITodoService? todoService = null)
    {
        return new ServiceCollection()
            .AddSingleton<ITodoService>(todoService ?? new InMemoryTodoService())
            .BuildServiceProvider();
    }
}
