namespace MigrationTool.Persistency;
public class CalendarEntity
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public List<EventEntity> Events { get; set; } = new();
}
