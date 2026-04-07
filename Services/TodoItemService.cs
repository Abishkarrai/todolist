using TodoList.Models;
using TodoList.Repositories;

namespace TodoList.Services;

public sealed class TodoItemService : ITodoItemService
{
    private readonly ITodoItemRepository _todoItemRepository;

    public TodoItemService(ITodoItemRepository todoItemRepository)
    {
        _todoItemRepository = todoItemRepository;
    }

    public Task<IReadOnlyList<TodoItem>> GetDashboardItemsAsync(CancellationToken cancellationToken)
    {
        return _todoItemRepository.GetAllAsync(cancellationToken);
    }

    public Task<TodoItem?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return _todoItemRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<bool> UpdateAsync(EditTodoItemInputModel input, CancellationToken cancellationToken)
    {
        return _todoItemRepository.UpdateTitleAsync(
            input.Id,
            input.Title.Trim(),
            cancellationToken);
    }

    public Task<bool> ToggleCompletedAsync(string id, CancellationToken cancellationToken)
    {
        return _todoItemRepository.ToggleCompletedAsync(id, cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        return _todoItemRepository.DeleteAsync(id, cancellationToken);
    }

    public Task CreateAsync(CreateTodoItemInputModel input, CancellationToken cancellationToken)
    {
        var todoItem = new TodoItem
        {
            Title = input.Title.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            IsCompleted = false
        };

        return _todoItemRepository.CreateAsync(todoItem, cancellationToken);
    }
}
