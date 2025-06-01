using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDFToolApp.Service;
using EDFToolApp.Store;
using Microsoft.Win32;
using Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EDFToolApp.ViewModel;

public partial class FileViewModel(
    FileDbService fileDbService,
    EDFStore edfStore) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<RecentFileItemViewModel> _recentFiles = [];

    [ObservableProperty]
    private ObservableCollection<ActionItemViewModel> _actionItems = [
        new ActionItemViewModel
        {
            Title = "Browser File",
            Description = "Browse for a file to open",
        },
    ];


    [RelayCommand]
    private async Task OpenFile(RecentFileItemViewModel recentFileItem)
    {
        string? path = recentFileItem.FilePath;

        if (path is null)
            return;

        // update File accessTime
        var item = await fileDbService.Get(path);
        if (item is not null)
        {
            item.AccessedTime = DateTime.Now;
            await fileDbService.Update(item);
        }

        OpenFile(path);
    }

    [RelayCommand]
    private async Task BrowserFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an EDF File",
            Filter = "EDF Files (*.edf)|*.edf|All Files (*.*)|*.*"
        };

        bool? result = dialog.ShowDialog();
        if (result is null || result.Value == false)
        {
            Debug.WriteLine($"File selection cancelled");
        }

        string selectedFilePath = dialog.FileName;

        var item = AddToRecentFiles(selectedFilePath);

        var model = await fileDbService.Get(selectedFilePath);
        if (model is null)
        {
            await fileDbService.Create(new RecentFileModel()
            {
                FilePath = item.FilePath,
                AccessedTime = item.AccessedTime,
            });
        }
        else
        {
            model.AccessedTime = item.AccessedTime;
            await fileDbService.Update(model);
        }

        OpenFile(selectedFilePath);
    }

    private void OpenFile(string filePath)
    {
        edfStore.OpenFile(filePath);
    }

    private RecentFileItemViewModel AddToRecentFiles(string filePath)
    {
        if (!RecentFiles.Any(f => filePath.Equals(f.FilePath, StringComparison.OrdinalIgnoreCase)))
        {
            var newItem = new RecentFileItemViewModel
            {
                Title = System.IO.Path.GetFileName(filePath),
                SubTitle = filePath,
                AccessedTime = DateTime.Now,
                FilePath = filePath
            };
            RecentFiles.Insert(0, newItem);
            while (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }

            return newItem;
        }
        else
        {
            var existingItem = RecentFiles.First(f => filePath.Equals(f.FilePath, StringComparison.OrdinalIgnoreCase));
            RecentFiles.Remove(existingItem);
            RecentFiles.Insert(0, existingItem);
            existingItem.AccessedTime = DateTime.Now;

            return existingItem;
        }
    }
}
