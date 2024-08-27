namespace MigrationTool.Persistency
{
    public class NewDepartmentEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<NewUserEntity> Users { get; set; } = new List<NewUserEntity>();
    }
}
