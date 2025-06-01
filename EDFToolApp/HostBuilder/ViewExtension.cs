using EDFToolApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EDFToolApp.HostBuilder;
public static class ViewExtension
{
    public static IHostBuilder AddViews(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton(f => new MainWindow { DataContext = f.GetRequiredService<MainViewModel>() });
        });
    }
}
