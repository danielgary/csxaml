namespace Csxaml.Samples.TodoApp;

public sealed class InMemoryTodoService : ITodoService
{
    private List<TodoItemModel> _items = CreateSeedItems();

    public List<TodoItemModel> LoadItems()
    {
        return _items.ToList();
    }

    public void SaveItems(IEnumerable<TodoItemModel> items)
    {
        _items = items.ToList();
    }

    private static List<TodoItemModel> CreateSeedItems()
    {
        return new List<TodoItemModel>
        {
            new("todo-1", "Draft plan", "Sketch the interaction model for controlled inputs.", false),
            new("todo-2", "Wire runtime", "Reconcile the renderer and adapter flow.", true),
            new("todo-3", "Write tests", "Cover metadata, generator, and runtime behavior.", false),
        };
    }
}
