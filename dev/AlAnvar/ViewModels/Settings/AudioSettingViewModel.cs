using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlAnvar.ViewModels;

public partial class AudioSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<AudioItem> ExistingAudioItems { get; set; }

    [ObservableProperty]
    public partial string AudiosPath { get; set; } = Settings.AudiosPath;

    [RelayCommand]
    public async Task GetAvailableAudios()
    {
        try
        {
            using var db = new AlAnvarDBContext();
            var allAudios = await db.Audios
                .Select(t => new AudioItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    PersianName = t.PersianName,
                    DirName = t.DirName,
                    Url = t.Url
                })
                .ToListAsync();

            ExistingAudioItems = new(allAudios
                .Where(t => Directory.Exists(Path.Combine(Settings.AudiosPath, t.DirName)) && Directory.EnumerateFiles(Path.Combine(Settings.AudiosPath, t.DirName)).Any())
                .ToList());
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    [RelayCommand]
    public async Task ChooseFolderAsync()
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(App.MainWindow.AppWindow.Id);
        var result = await picker.PickSingleFolderAsync();
        if (result is not null)
        {
            Settings.AudiosPath = result.Path;
            AudiosPath = result.Path;
        }
    }

    [RelayCommand]
    public async Task GoToFolderInExplorerAsync()
    {
        await GoToAudiosFolderInExplorerAsync();
    }

    [RelayCommand]
    public void GoToDownloadAudioPage()
    {
        App.Current.NavService.NavigateTo(typeof(AudiosPage));
    }
}

