using Microsoft.AspNetCore.Mvc;
using TodoList.Models;
using TodoList.Services;
using TodoList.ViewModels;

namespace TodoList.Controllers;

public sealed class TasksController : Controller
{
    private readonly ITodoItemService _todoItemService;

    public TasksController(ITodoItemService todoItemService)
    {
        _todoItemService = todoItemService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        TodoDashboardViewModel model = await BuildDashboardViewModelAsync(
                new CreateTodoItemInputModel(),
                cancellationToken)
            .ConfigureAwait(false);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        TodoItem? item = await _todoItemService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return NotFound();
        }

        return View(BuildEditViewModel(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTodoItemInputModel input, CancellationToken cancellationToken)
    {
        TodoItem? existingItem = await _todoItemService.GetByIdAsync(input.Id, cancellationToken).ConfigureAwait(false);
        if (existingItem is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(BuildEditViewModel(existingItem, input));
        }

        await _todoItemService.UpdateAsync(input, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "NewItem")] CreateTodoItemInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TodoDashboardViewModel invalidModel = await BuildDashboardViewModelAsync(input, cancellationToken)
                .ConfigureAwait(false);
            return View("Index", invalidModel);
        }

        await _todoItemService.CreateAsync(input, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompleted(string id, CancellationToken cancellationToken)
    {
        await _todoItemService.ToggleCompletedAsync(id, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _todoItemService.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    private async Task<TodoDashboardViewModel> BuildDashboardViewModelAsync(
        CreateTodoItemInputModel input,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoItem> items = await _todoItemService.GetDashboardItemsAsync(cancellationToken)
            .ConfigureAwait(false);

        return new TodoDashboardViewModel
        {
            NewItem = input,
            Items = items
        };
    }

    private static EditTodoItemViewModel BuildEditViewModel(
        TodoItem item,
        EditTodoItemInputModel? input = null)
    {
        return new EditTodoItemViewModel
        {
            Item = input ?? new EditTodoItemInputModel
            {
                Id = item.Id,
                Title = item.Title
            },
            IsCompleted = item.IsCompleted,
            CreatedAtUtc = item.CreatedAtUtc
        };
    }
}
