using System.Text.Json.Serialization;

namespace MigrationTool.Persistency;

public class NewToDoEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}