using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;
public partial class SignalViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string? _label;

    [ObservableProperty]
    private bool _isSelected;
}
