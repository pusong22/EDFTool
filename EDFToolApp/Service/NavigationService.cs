using EDFToolApp.Store;
using EDFToolApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace EDFToolApp.Service;

public class NavigationService(IServiceProvider provider, NavigationStore navigationStore)
{
    public void NavigationTo<TViewModel>() where TViewModel : BaseViewModel
    {
        var vm = provider.GetRequiredService<TViewModel>();
        navigationStore.CurrentViewModel = vm;
    }
}
