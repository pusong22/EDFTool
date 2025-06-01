using EDFToolApp.Router;
using EDFToolApp.Service;
using EDFToolApp.Store;
using EDFToolApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EDFToolApp.HostBuilder;
public static class StoreExtension
{
    public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(s =>
        {
            s.AddSingleton<NavigationStore>();
            s.AddSingleton<EDFStore>();

            s.AddSingleton(s =>
            {
                var navigationStore = s.GetRequiredService<NavigationStore>();

                return new ViewModelRouter(new Dictionary<string, Func<INavigationService>>
                {
                    {RouterName.FileView, () => new NavigationService<FileViewModel>(navigationStore, ()=> s.GetRequiredService<FileViewModel>()) }
                });
            });
        });
    }
}
