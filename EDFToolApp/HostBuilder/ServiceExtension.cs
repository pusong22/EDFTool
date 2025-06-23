using Microsoft.Extensions.Hosting;

namespace EDFToolApp.HostBuilder;
public static class ServiceExtension
{
    public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
        });
    }
}
