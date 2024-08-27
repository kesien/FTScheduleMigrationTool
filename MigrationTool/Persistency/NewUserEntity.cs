using Microsoft.AspNetCore.Identity;

namespace MigrationTool.Persistency
{
    public class NewUserEntity : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }
        public CalendarEntity Calendar { get; set; } = new();
        public Guid CalendarId { get; set; }
        public ICollection<NewDepartmentEntity> Departments { get; set; } = new List<NewDepartmentEntity>();
    }
}
