using System.Reflection;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MigrationTool;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var services = CreateServices();

        Application app = services.GetRequiredService<Application>();
        await app.Test();
    }

    private static ServiceProvider CreateServices()
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);
        var config = builder.Build();

        string connectionString = config.GetConnectionString("MyApp");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IContextFactory, ContextFactory>();
                services.AddSingleton<IDataProviderSelector, DataProviderSelector>();
                services.AddDbContext<ApplicationDbContext>(options =>
                services.BuildServiceProvider().GetRequiredService<IDataProviderSelector>().UseSql(options));
                services.AddScoped<ITransactionManager>(serviceProvider =>
                {
                    ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    return new TransactionManager(dbContext);
                });
                services.AddSingleton<Application>();
            })
            .Build();

        var app = ActivatorUtilities.CreateInstance<Application>(host.Services);

        await app.Test();
        var serviceProvider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            })
            .AddSingleton<Application>(new Application())
            .BuildServiceProvider();

        return serviceProvider;
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
