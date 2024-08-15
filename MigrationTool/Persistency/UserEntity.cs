using Microsoft.AspNetCore.Identity;

namespace MigrationTool.Persistency
{
    public class UserEntity : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }
        public ICollection<RequestEntity> Requests { get; set; } = new List<RequestEntity>();
        public ICollection<AbsenceEntity> Absences { get; set; } = new List<AbsenceEntity>();
        public ICollection<WorkAssignmentEntity> WorkAssignments { get; set; } = new List<WorkAssignmentEntity>();
        public ICollection<DepartmentEntity> Departments { get; set; } = new List<DepartmentEntity>();
    }
}
