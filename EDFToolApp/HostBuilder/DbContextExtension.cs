using EDFToolApp.EFDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace EDFToolApp.HostBuilder;
public static class DbContextExtension
{
    public static IHostBuilder AddDbContext(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
#if DEBUG
            services.AddSingleton<IDbContextFactory>(new MemoryDbContext());
#else
            string? dbFile = context.Configuration.GetConnectionString("sqlite") 
                            ?? throw new Exception("Connection string 'sqlite' is not configured in appsettings.json or environment variables.");
            string fullDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFile);

            fullDbPath = $"Data Source={fullDbPath}";
            services.AddSingleton<IDbContextFactory>(new FileDbContextFactory(fullDbPath));
#endif
        });
    }
}
