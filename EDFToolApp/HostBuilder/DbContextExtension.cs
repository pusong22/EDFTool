using EDFToolApp.EFDbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            string? connectionString = context.Configuration.GetConnectionString("sqlite") ?? throw new Exception("Connection string 'sqlite' is not configured in appsettings.json or environment variables.");

            services.AddSingleton<IDbContextFactory>(new FileDbContextFactory(connectionString));
#endif
        });
    }
}
