using MigrationTool.Constants;

namespace MigrationTool.Persistency
{
    public class AbsenceEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserEntity? User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AbsenceType Type { get; set; }
    }
}
