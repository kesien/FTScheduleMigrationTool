using Microsoft.EntityFrameworkCore;

namespace MigrationTool;

public interface IDataProviderSelector
{
    void UseSql(DbContextOptionsBuilder optionsBuilder);
}