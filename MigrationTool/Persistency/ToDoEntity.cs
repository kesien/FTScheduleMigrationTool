namespace MigrationTool.Persistency;

public class ToDoEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<WorkAssignmentEntity> WorkAssignments { get; set; } = new List<WorkAssignmentEntity>();
}