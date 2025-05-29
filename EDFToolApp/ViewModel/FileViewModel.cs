using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

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

    private void LoadFile(string filePath)
    {

    }

    private void AddToRecentFiles(string filePath)
    {
        if (!RecentFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
        {
            RecentFiles.Insert(0, new RecentFileItem
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(filePath),
                SubTitle = filePath,
                AccessedTime = DateTime.Now,
                FilePath = filePath
            });
            while (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }
        else
        {
            var existingItem = RecentFiles.First(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            RecentFiles.Remove(existingItem);
            RecentFiles.Insert(0, existingItem);
            existingItem.AccessedTime = DateTime.Now;
        }
    }

    [RelayCommand]
    private void OpenFile(RecentFileItem? recentFileItem)
    {
        Debug.WriteLine($"Click File: {recentFileItem?.FilePath}");
        MessageBox.Show(recentFileItem?.FilePath);

        //...
    }

    [RelayCommand]
    private void BrowserFile()
    {
        Debug.WriteLine($"Executing WIN32 OPENFILEDIALOG...");

        var dialog = new OpenFileDialog
        {
            Title = "Select an EDF File",
            Filter = "EDF Files (*.edf)|*.edf|All Files (*.*)|*.*"
        };

        bool? result = dialog.ShowDialog();
        if (result is not null && result.Value)
        {
           string selectedFilePath = dialog.FileName;

            Debug.WriteLine($"Selected File: {selectedFilePath}");

            DateTime currentDateTime = DateTime.Now;

            LoadFile(selectedFilePath);

            AddToRecentFiles(selectedFilePath);

        }
        else
        {
            Debug.WriteLine($"File selection cancelled");
        }
    }

}
