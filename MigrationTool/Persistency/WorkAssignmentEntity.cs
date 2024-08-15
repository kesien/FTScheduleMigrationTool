namespace MigrationTool.Persistency
{
    public class WorkAssignmentEntity
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid UserId { get; set; }
        public Guid SeatId { get; set; }
        public bool IsRequest { get; set; }
        public UserEntity? User { get; set; }
        public Guid WorkScheduleId { get; set; }
        public WorkScheduleEntity? WorkSchedule { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int DayIndex { get; set; }
        public ICollection<ToDoEntity> Todos { get; set; } = new List<ToDoEntity>();
    }
}
