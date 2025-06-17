using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDFToolApp.Service;
using EDFToolApp.Store;
using EDFToolApp.View;

namespace EDFToolApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;
    private readonly NavigationService _navigationService;
    private readonly NavigationWindowService _navigationWindowService;

    public MainViewModel(NavigationStore navigationStore,
        NavigationService navigationService,
        NavigationWindowService navigationWindowService)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        CurrentViewModel = _navigationStore.CurrentViewModel;

        _navigationService = navigationService;
        _navigationWindowService = navigationWindowService;
    }


    [ObservableProperty]
    private BaseViewModel? _currentViewModel;

    [ObservableProperty]
    private bool isPaneOpen = true;

    [RelayCommand]
    private void ShowSignalList()
    {
        _navigationWindowService.Show<SignalSelectorView>();
        //_navigationService.NavigationTo<SignalSelectorViewModel>();
    }


    private void OnCurrentViewModelChanged()
    {
        CurrentViewModel = _navigationStore.CurrentViewModel;
    }

    protected override void Dispose(bool disposing)
    {
        _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
    }
}
