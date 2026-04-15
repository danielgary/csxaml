using Csxaml.Demo;

namespace Csxaml.Runtime.Tests.Demo;

internal sealed class RecordingTodoService : ITodoService
{
    private List<TodoItemModel> _items;

    public RecordingTodoService(IEnumerable<TodoItemModel> items)
    {
        _items = items.ToList();
    }

    public List<TodoItemModel> LoadItems()
    {
        return _items.ToList();
    }

    public void SaveItems(IEnumerable<TodoItemModel> items)
    {
        _items = items.ToList();
    }

    public List<TodoItemModel> Snapshot()
    {
        return _items.ToList();
    }
}
