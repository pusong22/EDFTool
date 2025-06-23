using EdfViewerApp.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EdfViewerApp.HostBuilder;
public static class StoreExtension
{
    public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<EDFStore>();
        });
    }
}
