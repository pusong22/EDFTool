using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;

public partial class RecentFileItemViewModel : BaseViewModel
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
