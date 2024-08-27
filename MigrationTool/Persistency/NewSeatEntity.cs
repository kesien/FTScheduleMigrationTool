namespace MigrationTool.Persistency;

public class NewSeatEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public NewLocationEntity? Location { get; set; }
    public Guid LocationId { get; set; }
}