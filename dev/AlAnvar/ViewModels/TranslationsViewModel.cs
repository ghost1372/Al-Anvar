using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class TranslationsViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private CancellationTokenSource? _token;

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool ShowTranslationNotFound { get; set; }

    [ObservableProperty]
    public partial string Query { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TranslationItem> TranslationItems { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TranslationItem> LocalTranslationItems { get; set; }

    public async Task DeleteSelections(List<TranslationItem> translationItems)
    {
        try
        {
            foreach (var item in translationItems)
            {
                var filePath = Path.Combine(Settings.TranslationsPath, item.Id + ".json");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            await MessageBox.ShowInfoAsync(Strings.DownloadTranslationsViewModel_SuccessDelete.GetLocalizedResource(), Strings.DownloadTranslationsViewModel_SuccessDeleteTitle.GetLocalizedResource());
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    public async Task GetAvailableTranslations()
    {
        IsActive = true;

        await Task.Run(async () =>
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
                        Link = t.Link,
                        IsActive = false
                    })
                    .ToListAsync();

                foreach (var item in allTranslations)
                {
                    var filePath = Path.Combine(Settings.TranslationsPath, item.Id + ".json");
                    item.IsActive = File.Exists(filePath);
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    TranslationItems = new(allTranslations);
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

        IsActive = false;
    }
    
    public async Task GetAvailableLocalTranslations()
    {
        IsActive = true;

        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var allTranslations = await db.Translations.Select(t => new TranslationItem
                {
                    Id = t.TranslationId,
                    Name = t.Name,
                    Translator = t.Translator,
                    Language = t.Language,
                    Link = t.Link,
                    IsActive = false
                }).ToListAsync();

                foreach (var item in allTranslations)
                {
                    var filePath = Path.Combine(Settings.TranslationsPath, item.Id + ".json");
                    item.IsActive = File.Exists(filePath);
                }

                allTranslations = allTranslations.Where(x => x.IsActive == true).ToList();
                dispatcherQueue.TryEnqueue(() =>
                {
                    LocalTranslationItems = new(allTranslations);

                    if (LocalTranslationItems == null || LocalTranslationItems.Count == 0)
                    {
                        ShowTranslationNotFound = true;
                    }
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

        IsActive = false;
    }
    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
    }

    public async void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Query = sender.Text;

        if (_token is not null)
        {
            _token.Cancel();
        }

        _token = new CancellationTokenSource();
        await RefreshFilter(_token.Token);
    }

    internal bool Filter(object? item)
    {
        if (string.IsNullOrWhiteSpace(Query)) return true;
        if (item is null) return false;

        var model = (TranslationItem)item;

        return model.Name?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Translator?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Language?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true;
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
        TranslationsPage.Instance.GetTableView().RefreshFilter();
    }
}
