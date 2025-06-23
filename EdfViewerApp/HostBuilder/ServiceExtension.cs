using Microsoft.Extensions.Hosting;

namespace EdfViewerApp.HostBuilder;
public static class ServiceExtension
{
    public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
        });
    }
}
