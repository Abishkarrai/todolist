using System.ComponentModel.DataAnnotations;

namespace TodoList.Models;

public sealed class EditTodoItemInputModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;
}
