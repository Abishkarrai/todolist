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
        var model = await BuildDashboardViewModelAsync(new CreateTodoItemInputModel(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "NewItem")] CreateTodoItemInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildDashboardViewModelAsync(input, cancellationToken);
            return View("Index", invalidModel);
        }

        await _todoItemService.CreateAsync(input, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompleted(string id, CancellationToken cancellationToken)
    {
        await _todoItemService.ToggleCompletedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _todoItemService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task<TodoDashboardViewModel> BuildDashboardViewModelAsync(
        CreateTodoItemInputModel input,
        CancellationToken cancellationToken)
    {
        var items = await _todoItemService.GetDashboardItemsAsync(cancellationToken);

        return new TodoDashboardViewModel
        {
            NewItem = input,
            Items = items
        };
    }
}
