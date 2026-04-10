using GeneratedCsxaml;

namespace Csxaml.Runtime.Tests.Demo;

[TestClass]
public sealed class TodoDemoTests
{
    [TestMethod]
    public void TodoCard_DoneState_UsesGreenBackground()
    {
        var component = new TodoCardComponent();
        component.SetProps(new TodoCardProps("Ship milestone", true, () => { }));

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();

        Assert.AreEqual(TodoColors.DoneBackground, GetProperty<ArgbColor>(tree, "Background"));
    }

    [TestMethod]
    public void TodoCard_NotDoneState_UsesRedBackground()
    {
        var component = new TodoCardComponent();
        component.SetProps(new TodoCardProps("Ship milestone", false, () => { }));

        var tree = (NativeElementNode)new ComponentTreeCoordinator(component).Render();

        Assert.AreEqual(TodoColors.NotDoneBackground, GetProperty<ArgbColor>(tree, "Background"));
    }

    [TestMethod]
    public void TodoBoard_ToggleUpdatesCardBackground()
    {
        var component = new TodoBoardComponent();
        var coordinator = new ComponentTreeCoordinator(component);

        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        var firstCard = GetTodoCard(firstTree, 0);

        Assert.AreEqual(TodoColors.NotDoneBackground, GetProperty<ArgbColor>(firstCard, "Background"));

        GetButtonHandler(GetToggleButton(firstCard))();

        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        var secondCard = GetTodoCard(secondTree, 0);

        Assert.AreEqual(TodoColors.DoneBackground, GetProperty<ArgbColor>(secondCard, "Background"));
    }

    private static Action GetButtonHandler(NativeElementNode button)
    {
        return (Action)button.Events.Single(eventValue => eventValue.Name == "OnClick").Handler;
    }

    private static NativeElementNode GetButton(NativeElementNode cardStackPanel)
    {
        return (NativeElementNode)cardStackPanel.Children[^1];
    }

    private static T? GetProperty<T>(NativeElementNode node, string name)
    {
        var property = node.Properties.Single(propertyValue => propertyValue.Name == name).Value;
        return property is null ? default : (T)property;
    }

    private static NativeElementNode GetTodoCard(NativeElementNode board, int index)
    {
        return (NativeElementNode)board.Children[index + 1];
    }

    private static NativeElementNode GetToggleButton(NativeElementNode border)
    {
        var stackPanel = (NativeElementNode)border.Children.Single();
        return GetButton(stackPanel);
    }
}
