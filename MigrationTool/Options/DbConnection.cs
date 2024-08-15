using System.ComponentModel.DataAnnotations;

namespace MigrationTool.Options;

public class DbConnection
{
    [Required] public string? ConnectionString { get; set; }

    public string? Type { get; set; }
}