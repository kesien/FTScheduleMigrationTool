namespace MigrationTool.Persistency;

public class WorkScheduleEntity
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid LocationId { get; set; }
    public bool IsSaved { get; set; } = false;
    public LocationEntity? Location { get; set; }
    public ICollection<WorkAssignmentEntity> WorkAssignments { get; set; } = new List<WorkAssignmentEntity>();
}