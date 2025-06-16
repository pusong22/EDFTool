using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EDFToolApp.Message;
using EDFToolApp.Service;
using EDFToolApp.Store;
using Microsoft.Win32;
using Model;
using System.Collections.ObjectModel;

namespace EDFToolApp.ViewModel;
public partial class StartupWindowViewModel(
    FileDbService fileDbService,
    EDFStore edfStore) : BaseViewModel
{
    private readonly OpenFileDialog _openFileDialog = new();

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
    private async Task LoadRecentFiles()
    {
        RecentFiles.Clear();
        var recentFiles = await fileDbService.GetAll();
        foreach (var file in recentFiles)
        {
            RecentFiles.Add(new RecentFileItemViewModel
            {
                Title = System.IO.Path.GetFileName(file.FilePath),
                SubTitle = file.FilePath,
                AccessedTime = file.AccessedTime,
                FilePath = file.FilePath
            });
        }
    }

    [RelayCommand]
    private async Task OpenFile(RecentFileItemViewModel recentFileItem)
    {
        var path = recentFileItem.FilePath;
        if (string.IsNullOrWhiteSpace(path))
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
        _openFileDialog.Title = "Select an EDF File";
        _openFileDialog.Filter = "EDF Files (*.edf)|*.edf|All Files (*.*)|*.*";

        if (_openFileDialog.ShowDialog() != true) return;

        string selectedFilePath = _openFileDialog.FileName;
        var item = AddToRecentFiles(selectedFilePath);

        var model = await fileDbService.Get(selectedFilePath);
        if (model is null)
        {
            await fileDbService.Create(new RecentFileModel
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

        WeakReferenceMessenger.Default.Send(new RequestCloseWindowMessage(true));
    }

    private RecentFileItemViewModel AddToRecentFiles(string filePath)
    {
        var existingItem = RecentFiles.FirstOrDefault(f => filePath.Equals(f.FilePath, StringComparison.OrdinalIgnoreCase));
        if (existingItem is null)
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
            RecentFiles.Remove(existingItem);
            RecentFiles.Insert(0, existingItem);
            existingItem.AccessedTime = DateTime.Now;
            return existingItem;
        }
    }
}
