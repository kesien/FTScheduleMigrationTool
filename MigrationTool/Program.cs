using EntityRepository.Application.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTool.Options;

namespace MigrationTool;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);
        var config = builder.Build();

        string connectionString = config.GetConnectionString("MyApp");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddOptions<DbConnection>().Bind(config.GetSection("Database")).ValidateDataAnnotations();
                services.AddSingleton<IContextFactory, ContextFactory>();
                services.AddSingleton<IDataProviderSelector, DataProviderSelector>();
                services.AddDbContext<OldApplicationDbContext>(options =>
                    services.BuildServiceProvider().GetRequiredService<IDataProviderSelector>().UseSql(options));
                services.AddDbContext<NewApplicationDbContext>(opt =>
                {
                    opt.UseNpgsql(config.GetConnectionString("newContext"));
                });
                services.AddScoped<ITransactionManager>(serviceProvider =>
                {
                    OldApplicationDbContext dbContext = serviceProvider.GetRequiredService<OldApplicationDbContext>();
                    return new TransactionManager(dbContext);
                });
                services.AddSingleton<Application>();
            })
            .Build();

        var app = ActivatorUtilities.CreateInstance<Application>(host.Services);

        await app.Migrate();
    }


    private static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddEnvironmentVariables();
    }
}
