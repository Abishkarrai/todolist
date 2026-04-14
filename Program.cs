using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using TodoList.Configuration;
using TodoList.Repositories;
using TodoList.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    MongoDbSettings settings = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>()
        .Value;

    ILogger logger = serviceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("TodoList.MongoDb");

    IMongoClient client = new MongoClient(settings.ConnectionString);
    EnsureMongoConnectivity(client, settings.DatabaseName, logger, settings.ConnectionString);
    return client;
});

builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddControllersWithViews();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static void EnsureMongoConnectivity(
    IMongoClient client,
    string databaseName,
    ILogger logger,
    string connectionString)
{
    string preview = BuildConnectionPreview(connectionString);

    try
    {
        IMongoDatabase database = client.GetDatabase(databaseName);
        database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
    }
    catch (MongoAuthenticationException authenticationException)
    {
        StartupLogMessages.MongoAuthenticationFailed(logger, preview, authenticationException);
        throw new InvalidOperationException(
            "MongoDB authentication failed. Update MongoDb:ConnectionString so the SCRAM-SHA-1 credentials match your server.",
            authenticationException);
    }
    catch (MongoConfigurationException configurationException)
    {
        StartupLogMessages.MongoConfigurationFailed(logger, preview, configurationException);
        throw new InvalidOperationException(
            "MongoDB configuration is invalid. Ensure MongoDb:ConnectionString is a valid MongoDB URI.",
            configurationException);
    }
    catch (MongoException mongoException)
    {
        StartupLogMessages.MongoConnectivityFailed(logger, preview, mongoException);
        throw new InvalidOperationException(
            "Unable to connect to MongoDB. Confirm the server is reachable and the connection string is correct.",
            mongoException);
    }
}

static string BuildConnectionPreview(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return "<empty>";
    }

    return connectionString.Length <= 32
        ? connectionString
        : $"{connectionString[..32]}...";
}

internal static partial class StartupLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "MongoDB authentication failed during startup (preview: {preview})")]
    internal static partial void MongoAuthenticationFailed(ILogger logger, string preview, Exception exception);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "MongoDB configuration failed during startup (preview: {preview})")]
    internal static partial void MongoConfigurationFailed(ILogger logger, string preview, Exception exception);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Unable to reach MongoDB during startup (preview: {preview})")]
    internal static partial void MongoConnectivityFailed(ILogger logger, string preview, Exception exception);
}
