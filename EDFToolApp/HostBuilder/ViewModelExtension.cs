using EDFToolApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EDFToolApp.HostBuilder;
public static class ViewModelExtension
{
    public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<MainViewModel>();
            s.AddSingleton<StartupViewModel>();
            s.AddSingleton<SignalSelectorViewModel>();
        });
    }
}
