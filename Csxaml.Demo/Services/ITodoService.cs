using System.Collections.Generic;

namespace Csxaml.Demo;

public interface ITodoService
{
    List<TodoItemModel> LoadItems();

    void SaveItems(IEnumerable<TodoItemModel> items);
}
