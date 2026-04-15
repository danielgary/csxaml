using Csxaml.Demo;

namespace Csxaml.Runtime.Tests.Demo;

[TestClass]
public sealed class InMemoryTodoServiceTests
{
    [TestMethod]
    public void SaveItems_ReplacesPersistedSnapshotAndPreservesOrder()
    {
        var service = new InMemoryTodoService();

        service.SaveItems(
            new[]
            {
                new TodoItemModel("todo-1", "Draft plan", "Sketch the interaction model for controlled inputs.", false),
                new TodoItemModel("todo-2", "Updated title", "Updated notes", false),
                new TodoItemModel("todo-3", "Write tests", "Cover metadata, generator, and runtime behavior.", false)
            });
        var items = service.LoadItems();

        CollectionAssert.AreEqual(new[] { "todo-1", "todo-2", "todo-3" }, items.Select(item => item.Id).ToArray());
        Assert.AreEqual("Updated title", items[1].Title);
        Assert.AreEqual("Updated notes", items[1].Notes);
        Assert.IsFalse(items[1].IsDone);
    }

    [TestMethod]
    public void LoadItems_ReturnsCopyOfPersistedSnapshot()
    {
        var service = new InMemoryTodoService();
        var snapshot = service.LoadItems();

        snapshot[0] = snapshot[0] with { Title = "Mutated copy" };

        Assert.AreEqual("Draft plan", service.LoadItems()[0].Title);
    }
}
