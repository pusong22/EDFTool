using EDFToolApp.Store;
using EDFToolApp.ViewModel;

namespace EDFToolApp.Service;

public class NavigationService<TViewModel>(
    NavigationStore navigationStore,
    Func<TViewModel> createViewModel)
    : INavigationService where TViewModel : BaseViewModel
{
    public void NavigationTo()
    {
        navigationStore.CurrentViewModel = createViewModel();
    }
}
