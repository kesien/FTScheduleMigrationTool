using MigrationTool.Constants;

namespace MigrationTool.Persistency;

public class EventEntity
{
    public Guid Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Summary { get; set; }
    public DateTimeOffset? Date { get; set; } = null;
    public DateTimeOffset DtStart { get; set; }
    public DateTimeOffset DtEnd { get; set; }
    public bool IsAllDay { get; set; }
    public string Location { get; set; }
    public Guid LocationId { get; set; }
    public EventType EventType { get; set; } = EventType.Event;
    public string Todos { get; set; }
    public string? Description { get; set; }
    public string TimezoneId { get; set; }
    public DateTimeOffset? RecurrenceId { get; set; }
    public List<RecurrenceEntity> Recurrences { get; set; } = new();
    public List<RecurrenceExceptionEntity> Exceptions { get; set; } = new();
    public CalendarEntity Calendar { get; set; }
    public Guid SeatId { get; set; }
    public string? PhoneNumber { get; set; }
}
