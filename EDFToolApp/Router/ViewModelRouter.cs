using EDFToolApp.Service;

namespace EDFToolApp.Router;
public class ViewModelRouter(Dictionary<string, Func<INavigationService>> router)
{
    public void NavigateTo(string routeName)
    {
        if (!router.TryGetValue(routeName, out var serviceFactory))
        {
            throw new KeyNotFoundException($"No navigation service found for routeName: {routeName}");
        }

        var navigationService = serviceFactory();
        navigationService.NavigationTo();
    }
}
