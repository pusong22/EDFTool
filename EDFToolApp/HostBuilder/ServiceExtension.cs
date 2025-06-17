using EDFToolApp.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

namespace EDFToolApp.HostBuilder;
public static class ServiceExtension
{
    public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<EDFService>();
            s.AddSingleton<FileDbService>();
            s.AddSingleton<NavigationService>();
        });
    }
}
