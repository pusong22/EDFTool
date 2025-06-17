using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDFToolApp.Store;
using System.Collections.ObjectModel;
using System.IO;

namespace EDFToolApp.ViewModel;
public partial class SignalSelectorViewModel(EDFStore edfStore) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<SignalViewModel> _signals = [];

    [RelayCommand]
    private void LoadFakeSignals()
    {
        Signals.Clear();

        foreach (SignalViewModel signalViewModel in edfStore.ReadInfo())
        {
            Signals.Add(signalViewModel);
        }
    }

    [RelayCommand]
    private void SelectAll() => Signals.ToList().ForEach(s => s.IsSelected = true);
    [RelayCommand]
    private void ClearAll() => Signals.ToList().ForEach(s => s.IsSelected = false);

    [RelayCommand]
    private void SavePreset()
    {
        var selected = Signals.Where(s => s.IsSelected).Select(s => s.Label);
        File.WriteAllText("preset.txt", string.Join(",", selected));
    }

    [RelayCommand]
    private void LoadPreset()
    {
        if (!File.Exists("preset.txt")) return;
        var selected = File.ReadAllText("preset.txt").Split(',');

        foreach (var signal in Signals)
            signal.IsSelected = selected.Contains(signal.Label);
    }
}
