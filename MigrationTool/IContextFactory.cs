using Microsoft.EntityFrameworkCore;

namespace MigrationTool;

public interface IContextFactory
{
    public DbContext GetContext();
}