namespace Csxaml.Samples.TodoApp;

public interface ITodoService
{
    List<TodoItemModel> LoadItems();

    void SaveItems(IEnumerable<TodoItemModel> items);
}
