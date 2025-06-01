using CommunityToolkit.Mvvm.ComponentModel;
using EDFToolApp.Store;

namespace EDFToolApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;

    public MainViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        CurrentViewModel = _navigationStore.CurrentViewModel;
    }

    [ObservableProperty]
    private BaseViewModel? _currentViewModel;

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModel = _navigationStore.CurrentViewModel;
    }

    protected override void Dispose(bool disposing)
    {
        _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
    }
}
