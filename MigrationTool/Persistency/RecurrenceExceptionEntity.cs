namespace MigrationTool.Persistency;

public class RecurrenceExceptionEntity
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public EventEntity Event { get; set; }
}
