namespace MigrationTool.Persistency
{
    public class ApplicationLanguageEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? TranslationKey { get; set; }
    }
}
