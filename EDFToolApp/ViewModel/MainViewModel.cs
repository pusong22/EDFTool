using CommunityToolkit.Mvvm.ComponentModel;
using EDFToolApp.Store;

namespace EDFToolApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;
   

    public MainViewModel(NavigationStore navigationStore, SignalSelectorViewModel signalSelectorViewModel)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        CurrentViewModel = _navigationStore.CurrentViewModel;

        _signalSelectorViewModel = signalSelectorViewModel;
    }


    [ObservableProperty]
    private BaseViewModel? _currentViewModel;


    [ObservableProperty]
    private SignalSelectorViewModel? _signalSelectorViewModel;


    private void OnCurrentViewModelChanged()
    {
        CurrentViewModel = _navigationStore.CurrentViewModel;
    }

    protected override void Dispose(bool disposing)
    {
        _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
    }
}
