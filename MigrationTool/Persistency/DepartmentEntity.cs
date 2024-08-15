namespace MigrationTool.Persistency
{
    public class DepartmentEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    }
}
