using System.Diagnostics;

namespace AlAnvar.ViewModels;

public partial class DatabaseSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string DatabasePath { get; set; } = Settings.DBPath;

    [RelayCommand]
    public async Task ChooseFileAsync()
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(App.MainWindow.AppWindow.Id);
        picker.FileTypeChoices.Add("Database", new List<string>() { ".db" });
        var result = await picker.PickSingleFileAsync();
        if (result is not null)
        {
            Settings.DBPath = result.Path;
            DatabasePath = result.Path;
        }
    }

    public async void SaveFileAsync()
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FileSavePicker(App.MainWindow.AppWindow.Id);
        picker.FileTypeChoices.Add("Database", new List<string>() { ".db" });
        picker.SuggestedFileName = "Al-Anvar";
        var result = await picker.PickSaveFileAsync();
        if (result is not null)
        {
            File.Copy(Settings.DBPath, result.Path, true);
        }
    }

    public async void RestoreFileAsync()
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(App.MainWindow.AppWindow.Id);
        picker.FileTypeChoices.Add("Database", new List<string>() { ".db" });
        var result = await picker.PickSingleFileAsync();
        if (result is not null)
        {
            File.Copy(result.Path, Settings.DBPath, true);
        }
    }

    [RelayCommand]
    public async Task GoToFolderInExplorerAsync()
    {
        if (File.Exists(Settings.DBPath))
        {
            Process.Start("explorer.exe", $"/select,\"{Settings.DBPath}\"");
        }
        else
        {
            await MessageBox.ShowErrorAsync(Strings.DatabaseSettingViewModel_MessageBoxFileNotFound.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }
}
