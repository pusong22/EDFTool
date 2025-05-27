using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using System.Collections.ObjectModel;

namespace EDFToolApp.ViewModel;

public partial class FileViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RecentFileItem> _recentFiles = [];

    [ObservableProperty]
    private ObservableCollection<ActionItem> _actionItems = [];

    public FileViewModel()
    {
        RecentFiles = [
            new RecentFileItem {
                Title = "Project Alpha Report",
                SubTitle = "Detailed analysis of Q1 performance",
                AccessedTime = DateTime.Now.AddHours(-2),
                FilePath = "C:\\Temp\\ProjectAlphaReport.edf"
            },
        ];

        ActionItems = [
            new ActionItem{
                Title = "Open New File",
                Description = "Browse and open an EDF file.."
            }
        ];
    }

    [RelayCommand]
    private void OpenFile()
    {

    }
}
