using CommunityToolkit.Mvvm.ComponentModel;

namespace EdfViewerApp.ViewModel;
public partial class SignalViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? _label;

    [ObservableProperty]
    private bool _isSelected;

    public int Id { get; set; }
    public double SampleRate { get; set; }
}
