namespace MigrationTool.Persistency;

public class SeatEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public LocationEntity? Location { get; set; }
    public Guid LocationId { get; set; }
}