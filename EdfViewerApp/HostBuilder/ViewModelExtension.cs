using EdfViewerApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EdfViewerApp.HostBuilder;
public static class ViewModelExtension
{
    public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<MainViewModel>();
            s.AddSingleton<SignalSelectorViewModel>();
            s.AddSingleton<ChartViewModel>();
        });
    }
}
