using TodoList.Models;

namespace TodoList.Repositories;

public interface ITodoItemRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken);

    Task<TodoItem?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task CreateAsync(TodoItem item, CancellationToken cancellationToken);

    Task<bool> UpdateTitleAsync(string id, string title, CancellationToken cancellationToken);

    Task<bool> ToggleCompletedAsync(string id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
