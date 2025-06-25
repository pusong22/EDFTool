using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EdfViewerApp.HostBuilder;
public static class ViewExtension
{
    public static IHostBuilder AddViews(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<MainWindow>();
        });
    }
}
