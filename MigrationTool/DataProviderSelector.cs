using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MigrationTool.Options;

namespace MigrationTool;

public class DataProviderSelector : IDataProviderSelector
{
    private readonly string _connection;

    public DataProviderSelector(IOptionsMonitor<DbConnection> connection)
    {
        _connection = connection.CurrentValue.ConnectionString;
    }

    public void UseSql(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseNpgsql(_connection);
    }
}