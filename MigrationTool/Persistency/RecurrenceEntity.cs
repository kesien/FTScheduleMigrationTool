namespace MigrationTool.Persistency;

public class RecurrenceEntity
{
    private int _interval = int.MinValue;
    private DateTime _until = DateTime.MinValue;
    public Guid Id { get; set; }
    public string Frequency { get; set; }
    public DateTime Until
    {
        get => _until;
        set
        {
            if (_until == value && _until.Kind == value.Kind)
                return;
            _until = value;
        }
    }
    public int Count { get; set; } = int.MinValue;
    public int Interval
    {
        get => _interval != int.MinValue ? _interval : 1;
        set => _interval = value;
    }
    public List<int> BySecond { get; set; } = new();
    public List<int> ByMinute { get; set; } = new();
    public List<int> ByHour { get; set; } = new();
    public List<int> ByDay { get; set; } = new();
    public List<int> ByMonthDay { get; set; } = new();
    public List<int> ByYearDay { get; set; } = new();
    public List<int> ByWeekNo { get; set; } = new();
    public List<int> ByMonth { get; set; } = new();
    public List<int> BySetPosition { get; set; } = new();

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    public string RecurrencePattern { get; set; }
    public EventEntity Event { get; set; }
}
