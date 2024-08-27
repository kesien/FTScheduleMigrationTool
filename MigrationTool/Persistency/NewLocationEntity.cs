namespace MigrationTool.Persistency
{
    public class NewLocationEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public ICollection<NewSeatEntity> Seats { get; set; } = new List<NewSeatEntity>();
    }
}
