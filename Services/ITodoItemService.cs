using TodoList.Models;

namespace TodoList.Services;

public interface ITodoItemService
{
    Task<IReadOnlyList<TodoItem>> GetDashboardItemsAsync(CancellationToken cancellationToken);

    Task<TodoItem?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task CreateAsync(CreateTodoItemInputModel input, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(EditTodoItemInputModel input, CancellationToken cancellationToken);

    Task<bool> ToggleCompletedAsync(string id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
