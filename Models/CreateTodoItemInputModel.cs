using System.ComponentModel.DataAnnotations;

namespace TodoList.Models;

public sealed class CreateTodoItemInputModel
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;
}
