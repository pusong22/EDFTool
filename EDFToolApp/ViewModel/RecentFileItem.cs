using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;

public partial class RecentFileItem : ObservableObject
{
    [ObservableProperty]
    private string? title;
    [ObservableProperty]
    private string? subTitle;
    [ObservableProperty]
    private DateTime accessedTime;
    [ObservableProperty]
    private string? filePath;
}
