using System.ComponentModel.DataAnnotations;

namespace TodoList.Configuration;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = string.Empty;

    [Required]
    public string TodoCollectionName { get; set; } = string.Empty;
}
