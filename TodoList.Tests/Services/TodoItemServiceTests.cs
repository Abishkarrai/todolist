using Moq;
using NUnit.Framework;
using TodoList.Models;
using TodoList.Repositories;
using TodoList.Services;

namespace TodoList.Tests.Services;

[TestFixture]
public sealed class TodoItemServiceTests
{
    [Test]
    public async Task CreateAsync_TrimsTitle_AndCreatesIncompleteItem()
    {
        var repositoryMock = new Mock<ITodoItemRepository>(MockBehavior.Strict);
        TodoItem? capturedItem = null;

        repositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .Callback<TodoItem, CancellationToken>((item, _) => capturedItem = item)
            .Returns(Task.CompletedTask);

        var service = new TodoItemService(repositoryMock.Object);
        var before = DateTime.UtcNow;

        await service.CreateAsync(
            new CreateTodoItemInputModel
            {
                Title = "  Learn NUnit  "
            },
            CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.That(capturedItem, Is.Not.Null);
        Assert.That(capturedItem!.Title, Is.EqualTo("Learn NUnit"));
        Assert.That(capturedItem.IsCompleted, Is.False);
        Assert.That(capturedItem.CreatedAtUtc, Is.InRange(before, after));
        repositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task UpdateAsync_TrimsTitle_AndDelegatesToRepository()
    {
        var repositoryMock = new Mock<ITodoItemRepository>(MockBehavior.Strict);
        var input = new EditTodoItemInputModel
        {
            Id = "0123456789abcdef01234567",
            Title = "  Updated title  "
        };

        repositoryMock
            .Setup(repository => repository.UpdateTitleAsync(input.Id, "Updated title", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new TodoItemService(repositoryMock.Object);

        var result = await service.UpdateAsync(input, CancellationToken.None);

        Assert.That(result, Is.True);
        repositoryMock.Verify(repository => repository.UpdateTitleAsync(input.Id, "Updated title", It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }
}
