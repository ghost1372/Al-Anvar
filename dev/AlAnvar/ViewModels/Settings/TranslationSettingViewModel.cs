using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlAnvar.ViewModels;

public partial class TranslationSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<TranslationItem> ExistingTranslationItems { get; set; }

    [ObservableProperty]
    public partial string TranslationsPath { get; set; } = Settings.TranslationsPath;

    [RelayCommand]
    public async Task GetAvailableTranslations()
    {
        try
        {
            using var db = new AlAnvarDBContext();
            var allTranslations = await db.Translations
                .Select(t => new TranslationItem
                {
                    Id = t.TranslationId,
                    Name = t.Name,
                    Translator = t.Translator,
                    Language = t.Language,
                    Link = t.Link
                })
                .ToListAsync();

            ExistingTranslationItems = new(allTranslations
                .Where(t => File.Exists(Path.Combine(Settings.TranslationsPath, t.Id + ".json")))
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
            Settings.TranslationsPath = result.Path;
            TranslationsPath = result.Path;
        }
    }

    [RelayCommand]
    public async Task GoToFolderInExplorerAsync()
    {
        await GoToTranslationsFolderInExplorerAsync();
    }

    [RelayCommand]
    public void GoToDownloadTranslationPage()
    {
        App.Current.NavService.NavigateTo(typeof(TranslationsPage));
    }
}
