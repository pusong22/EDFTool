using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdfViewerApp.Store;
using Microsoft.Win32;

namespace EdfViewerApp.ViewModel;

public partial class MainViewModel(EDFStore edfStore,
    SignalSelectorViewModel signalSelectorViewModel,
    LineSeriesViewModel lineSeriesViewModel,
    HeatSeriesViewModel heatSeriesViewModel) : BaseViewModel
{
    [ObservableProperty]
    private SignalSelectorViewModel? _signalSelectorViewModel = signalSelectorViewModel;

    [ObservableProperty]
    private LineSeriesViewModel? _lineSeriesViewModel = lineSeriesViewModel;

    [ObservableProperty]
    private HeatSeriesViewModel? _heatSeriesViewModel = heatSeriesViewModel;


    [RelayCommand]
    private void OpenEdfFile()
    {
        OpenFileDialog ofd = new()
        {
            Title = "Select an EDF File",
            Filter = "EDF Files (*.edf)|*.edf|All Files (*.*)|*.*"
        };


        if (ofd.ShowDialog() != true) return;

        string selectedFilePath = ofd.FileName;

        edfStore.OpenFile(selectedFilePath);

        SignalSelectorViewModel?.LoadSignalsCommand.Execute(null);
    }

    [RelayCommand]
    private void CloseEdfFile()
    {
        if (string.IsNullOrEmpty(edfStore.EdfFilePath)) return;

        SignalSelectorViewModel?.ClearSignalsCommand.Execute(null);
    }
}
