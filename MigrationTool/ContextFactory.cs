using Microsoft.EntityFrameworkCore;

namespace MigrationTool;

public class ContextFactory : IContextFactory
{
    private readonly IDataProviderSelector _dataProviderSelector;

    public ContextFactory(IDataProviderSelector dataProviderSelector)
    {
        _dataProviderSelector = dataProviderSelector;
    }

    public DbContext GetContext()
    {
        ApplicationDbContext ret = new(_dataProviderSelector);
        _ = ret.Database.EnsureCreated();
        return ret;
    }
}