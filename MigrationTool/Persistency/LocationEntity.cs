namespace MigrationTool.Persistency
{
    public class LocationEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public ICollection<SeatEntity> Seats { get; set; } = new List<SeatEntity>();
        public ICollection<RequestEntity> Requests { get; set; } = new List<RequestEntity>();
        public ICollection<WorkScheduleEntity> WorkSchedules { get; set; } = new List<WorkScheduleEntity>();
        public ICollection<ToDoEntity> ToDos { get; set; } = new List<ToDoEntity>();
    }
}
