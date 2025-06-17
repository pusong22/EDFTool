using EDFToolApp.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EDFToolApp.HostBuilder;
public static class ViewExtension
{
    public static IHostBuilder AddViews(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<MainWindow>();
            s.AddSingleton<StartupView>();
            s.AddSingleton<SignalSelectorView>();

            s.AddTransient<GenericWindow>();
        });
    }
}
