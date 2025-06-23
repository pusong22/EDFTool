using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using EdfViewerApp.Store;
using System.Collections.ObjectModel;

namespace EdfViewerApp.ViewModel;
public partial class SignalSelectorViewModel(EDFStore edfStore) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<SignalViewModel> _signals = [];

    [RelayCommand]
    private void LoadSignals()
    {
        if (!edfStore.Open) return;

        Signals.Clear();

        foreach (SignalViewModel signalViewModel in edfStore.ReadInfo())
        {
            Signals.Add(signalViewModel);
        }
    }

    [RelayCommand]
    private void ClearSignals()
    {
        Signals.Clear();
    }

    [RelayCommand]
    private void SelectAll() => Signals.ToList().ForEach(s => s.IsSelected = true);
    [RelayCommand]
    private void ClearAll() => Signals.ToList().ForEach(s => s.IsSelected = false);

    [RelayCommand]
    private void Plot()
    {
        var selectedSignals = Signals.Where(x => x.IsSelected);
        ValueChangedMessage<IEnumerable<SignalViewModel>> msg = new(selectedSignals);
        WeakReferenceMessenger.Default.Send(msg);
    }
}
