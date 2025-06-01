using EDFToolApp.ViewModel;

namespace EDFToolApp.Store;
public class NavigationStore
{
    private BaseViewModel? _currentViewModel;

    public BaseViewModel? CurrentViewModel
    {
        get { return _currentViewModel; }
        set
        {
            if (value != _currentViewModel)
            {
                _currentViewModel?.Dispose();
                _currentViewModel = value;

                OnCurrentViewModelChanged();
            }
        }
    }

    public event Action? CurrentViewModelChanged;

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}
