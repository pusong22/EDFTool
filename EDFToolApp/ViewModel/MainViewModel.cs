using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private FileViewModel _fileViewModel = new();
}
