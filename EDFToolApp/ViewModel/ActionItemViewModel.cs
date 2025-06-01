using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;

public partial class ActionItemViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? title;
    [ObservableProperty]
    private string? description;
}
