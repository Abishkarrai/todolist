using TodoList.Models;

namespace TodoList.ViewModels;

public sealed class EditTodoItemViewModel
{
    public EditTodoItemInputModel Item { get; init; } = new();

    public bool IsCompleted { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
