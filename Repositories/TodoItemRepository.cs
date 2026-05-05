using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TodoList.Configuration;
using TodoList.Models;

namespace TodoList.Repositories;

public sealed class TodoItemRepository : ITodoItemRepository
{
    private readonly IMongoCollection<TodoItem> _todoItems;

    public TodoItemRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        IMongoDatabase database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _todoItems = database.GetCollection<TodoItem>(settings.Value.TodoCollectionName);
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _todoItems
            .Find(Builders<TodoItem>.Filter.Empty)
            .SortByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TodoItem?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await _todoItems
            .Find(todoItem => todoItem.Id == id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task CreateAsync(TodoItem item, CancellationToken cancellationToken)
    {
        return _todoItems.InsertOneAsync(item, cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateTitleAsync(string id, string title, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        UpdateDefinition<TodoItem> update = Builders<TodoItem>.Update.Set(todoItem => todoItem.Title, title);
        UpdateResult result = await _todoItems.UpdateOneAsync(
            todoItem => todoItem.Id == id,
            update,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.MatchedCount > 0;
    }

    public async Task<bool> ToggleCompletedAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        TodoItem? item = await _todoItems
            .Find(todoItem => todoItem.Id == id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (item is null)
        {
            return false;
        }

        item.IsCompleted = !item.IsCompleted;

        ReplaceOneResult result = await _todoItems.ReplaceOneAsync(
            todoItem => todoItem.Id == id,
            item,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        DeleteResult result = await _todoItems.DeleteOneAsync(
            todoItem => todoItem.Id == id,
            cancellationToken)
            .ConfigureAwait(false);

        return result.DeletedCount > 0;
    }
}
