namespace MigrationTool.Persistency
{
    public class RequestEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserEntity? User { get; set; }
        public Guid LocationId { get; set; }
        public LocationEntity? Location { get; set; }
        public Guid SeatId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsFix { get; set; }
        public int DayIndex { get; set; }
    }
}
