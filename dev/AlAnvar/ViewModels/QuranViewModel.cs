using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class QuranViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    private CancellationTokenSource? _token;

    [ObservableProperty]
    public partial string Query { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuranMetadataTable> QuranMetaItems { get; set; }

    [ObservableProperty]
    public partial bool HasTabItems { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FinalQuran> QuranVerses { get; set; }

    [ObservableProperty]
    public partial string TitleNote { get; set; }
    partial void OnTitleNoteChanged(string value)
    {
        CanSaveNote = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(DescriptionNote);
    }

    [ObservableProperty]
    public partial string DescriptionNote { get; set; }
    partial void OnDescriptionNoteChanged(string value)
    {
        CanSaveNote = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(TitleNote);
    }

    [ObservableProperty]
    public partial bool CanSaveNote { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TranslationItem> ExistingTranslationItems { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<AudioItem> ExistingAudioItems { get; set; }

    [ObservableProperty]
    public partial bool IsAyaVisible { get; set; } = true;
    partial void OnIsAyaVisibleChanged(bool value)
    {
        ProxyService.Instance.IsAyaVisible = value;
    }

    [ObservableProperty]
    public partial bool IsTranslationVisible { get; set; } = true;
    partial void OnIsTranslationVisibleChanged(bool value)
    {
        ProxyService.Instance.IsTranslationVisible = value;
    }

    [ObservableProperty]
    public partial bool IsDiacriticsVisible { get; set; } = true;
    partial void OnIsDiacriticsVisibleChanged(bool value)
    {
        ProxyService.Instance.IsDiacriticsVisible = value;
    }

    [ObservableProperty]
    public partial bool IsOperationButtonVisible { get; set; } = true;
    partial void OnIsOperationButtonVisibleChanged(bool value)
    {
        ProxyService.Instance.IsOperationButtonVisible = value;
    }

    [ObservableProperty]
    public partial bool IsAutoPlayNextFile { get; set; } = true;

    [ObservableProperty]
    public partial TextAlignment VerseTextAlignment { get; set; } = TextAlignment.Right;
    partial void OnVerseTextAlignmentChanged(TextAlignment value)
    {
        ProxyService.Instance.VerseTextAlignment = value;
    }

    [ObservableProperty]
    public partial TextAlignment TranslationTextAlignment { get; set; } = TextAlignment.Right;
    partial void OnTranslationTextAlignmentChanged(TextAlignment value)
    {
        ProxyService.Instance.TranslationTextAlignment = value;
    }

    public async Task OnPageLoaded()
    {
        IsActive = true;

        await GetQuranMetaData();
        await GetAvailableTranslations();
        await GetAvailableAudios();

        IsActive = false;
    }

    public async Task GetQuranVerse(QuranMetadataTable quranMetadataTable)
    {
        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var result = await Queries.GetMixedQuranByIdQueryAsync(db, quranMetadataTable.Id, quranMetadataTable.Name, quranMetadataTable.FinglishName).ToListAsync();
                
                dispatcherQueue.TryEnqueue(() =>
                {
                    QuranVerses = new(result);
                });
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
                dispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });
    }

    public async Task GetQuranMetaData()
    {
        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var chapters = await Queries.GetAllChaptersQueryAsync(db).ToListAsync();

                dispatcherQueue.TryEnqueue(() =>
                {
                    QuranMetaItems = new(chapters);
                });
            }
            catch (Exception ex)
            {
                IsActive = false;
                Logger?.Error(ex, ex.Message);
                dispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });
    }

    
    [RelayCommand]
    public void GoToTranslationsPage()
    {
        EnsureNavigationSelection(typeof(TranslationsPage));
    }

    [RelayCommand]
    public async Task OpenTranslationsFolder()
    {
        await GoToTranslationsFolderInExplorerAsync();
    }

    [RelayCommand]
    public void GoToAudiosPage()
    {
        EnsureNavigationSelection(typeof(AudiosPage));
    }

    [RelayCommand]
    public async Task OpenAudiosFolder()
    {
        await GoToAudiosFolderInExplorerAsync();
    }

    public async Task GetAvailableTranslations()
    {
        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var allTranslations = await Queries.GetAllTranslationsQueryAsync(db)
                    .Select(t => new TranslationItem
                    {
                        Id = t.TranslationId,
                        Name = t.Name,
                        Translator = t.Translator,
                        Language = t.Language,
                        Link = t.Link
                    })
                    .ToListAsync();

                dispatcherQueue.TryEnqueue(() =>
                {
                    ExistingTranslationItems = new(allTranslations
                    .Where(t => File.Exists(Path.Combine(Settings.TranslationsPath, t.Id + ".json")))
                    .ToList());
                });
            }
            catch (Exception ex)
            {
                IsActive = false;
                Logger?.Error(ex, ex.Message);
                dispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });
    }

    public async Task GetAvailableAudios()
    {
        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var allAudios = await Queries.GetAllAudiosQueryAsync(db)
                    .Select(t => new AudioItem
                    {
                        Id = t.Id,
                        Name = t.Name,
                        PersianName = t.PersianName,
                        DirName = t.DirName,
                        Url = t.Url
                    })
                    .ToListAsync();

                dispatcherQueue.TryEnqueue(() =>
                {
                    ExistingAudioItems = new(allAudios
                    .Where(t => Directory.Exists(Path.Combine(Settings.AudiosPath, t.DirName)) && Directory.EnumerateFiles(Path.Combine(Settings.AudiosPath, t.DirName)).Any())
                    .ToList());
                });
            }
            catch (Exception ex)
            {
                IsActive = false;
                Logger?.Error(ex, ex.Message);
                dispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });
    }

    [RelayCommand]
    public void ChangeTabWidthMode(TabView tabView)
    {
        switch (Settings.TabWidthMode)
        {
            case TabViewWidthMode.Equal:
                Settings.TabWidthMode = TabViewWidthMode.SizeToContent;
                tabView.TabWidthMode = TabViewWidthMode.SizeToContent;
                break;
            case TabViewWidthMode.SizeToContent:
                Settings.TabWidthMode = TabViewWidthMode.Compact;
                tabView.TabWidthMode = TabViewWidthMode.Compact;
                break;
            case TabViewWidthMode.Compact:
                Settings.TabWidthMode = TabViewWidthMode.Equal;
                tabView.TabWidthMode = TabViewWidthMode.Equal;
                break;
            default:
                Settings.TabWidthMode = TabViewWidthMode.SizeToContent;
                tabView.TabWidthMode = TabViewWidthMode.SizeToContent;
                break;
        }
    }

    public async void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Query = sender.Text;

        TafsirTabViewItem.Instance?.ViewModel?.Query = sender.Text;

        if (_token is not null)
        {
            _token.Cancel();
        }

        _token = new CancellationTokenSource();
        await RefreshFilter(_token.Token);
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        
    }

    internal bool Filter(object? item)
    {
        if (string.IsNullOrWhiteSpace(Query)) return true;
        if (item is null) return false;

        var model = (FinalQuran)item;

        return model.CleanAya?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Translation?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true;
    }

    private async Task RefreshFilter(CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);
        }
        catch
        {
            return;
        }

        _token = null;

        var tableView = QuranPage.Instance.GetTableView();
        if (tableView != null)
        {
            tableView.RefreshFilter();
        }
    }
}
