using TodoList.Models;

namespace TodoList.ViewModels;

public sealed class TodoDashboardViewModel
{
    public CreateTodoItemInputModel NewItem { get; init; } = new();

    public IReadOnlyList<TodoItem> Items { get; init; } = [];

    public int CompletedCount => Items.Count(item => item.IsCompleted);

    public int OpenCount => Items.Count - CompletedCount;
}
