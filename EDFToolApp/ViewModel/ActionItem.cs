using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;

public partial class ActionItem : ObservableObject
{
    [ObservableProperty]
    private string? title;
    [ObservableProperty]
    private string? description;
}
