using Newtonsoft.Json;

namespace AlAnvar.ViewModels;

public partial class QuranViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    public QuranViewModel()
    {
        LoadQuranAsync();
        LoadTranslationAsync();

        Task.Run(() =>
        {
            LoadQaris();
            GetDefaultFont();
            GetDefaultForeground();
        });
    }

    private void GetDefaultForeground()
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (Settings.AyatForeground is not null)
            {
                AyatForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
            }
            if (Settings.AyatNumberForeground is not null)
            {
                AyatNumberForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
            }
            if (Settings.TranslationForeground is not null)
            {
                TranslationForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
            }
        });
    }

    private void GetDefaultFont()
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            AyatFontFamily = new FontFamily(Settings.AyatFontFamilyName);
            AyatNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
            TranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

            AyatFontSize = Settings.AyatFontSize;
            AyatNumberFontSize = Settings.AyatNumberFontSize;
            TranslationFontSize = Settings.TranslationFontSize;
        });
    }

    private async void LoadQuranAsync()
    {
        IsActive = true;
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();
            Chapters = new(await db.Chapters.ToListAsync());
            currentSortDescription = new SortDescription("Id", SortDirection.Ascending);
            dispatcherQueue.TryEnqueue(() =>
            {
                ChaptersACV = new AdvancedCollectionView(Chapters, true);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);
                suggestListForSurahSearch = ChaptersACV.Select(x => ((ChapterProperty) x).Name).ToList();
            });
        });
        IsActive = false;
    }

    private void LoadQaris()
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (Directory.Exists(Settings.AudiosPath))
            {
                var files = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var audios = JsonConvert.DeserializeObject<QuranAudio>(File.ReadAllText(file));
                            if (audios is not null)
                            {
                                QarisCollection.Add(audios);
                            }
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                }

                if (QarisCollection.Any())
                {
                    CurrentQari = QarisCollection.Where(audio => ((QuranAudio) audio).DirName == Settings.QuranAudio?.DirName).FirstOrDefault();
                    QariIndex = QarisCollection.IndexOf(CurrentQari);
                }
            }
        });
    }

    private async void LoadTranslationAsync()
    {
        IsActive = true;
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();
            var data = await db.Translations.Where(x => x.IsActive).ToListAsync();
            dispatcherQueue.TryEnqueue(() =>
            {
                TranslationsCollection = new(data);

                if (Settings.QuranTranslation != null)
                {
                    CurrentTranslation = TranslationsCollection.FirstOrDefault(x => x.Id == Settings.QuranTranslation.Id);
                    TranslationIndex = TranslationsCollection.IndexOf(CurrentTranslation);
                }
            });
        });
        IsActive = false;
    }

    public void Search(AutoSuggestBox sender)
    {
        if (ChaptersACV != null)
        {
            ChaptersACV.Filter = _ => true;
            ChaptersACV.Filter = chapter =>
            {
                var query = chapter as ChapterProperty;

                var name = query.Name ?? "";
                var tName = query.TName ?? "";
                var type = query.Type ?? "";
                var aya = query.Ayas.ToString() ?? "";

                return name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                        || tName.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                        || aya.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                        || type.Contains(sender.Text, StringComparison.OrdinalIgnoreCase);
            };
        }
    }

    private void AddNewSurahTab(TabView tabView, ChapterProperty chapterProperty)
    {
        this.tabview = tabView;
        var currentTabViewItem = tabView.TabItems?.Where(tabViewItem => (tabViewItem as QuranTabViewItem)?.Chapter?.Id == chapterProperty.Id)?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{chapterProperty.Id} - {chapterProperty.Name} - {chapterProperty.Ayas} آیه - {chapterProperty.Type}";
        item.Chapter = chapterProperty;
        tabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }

    private void TabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        tabview.TabItems.Remove(sender);
        ListViewSelectedIndex = -1;
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Search(sender);
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        Search(sender);
    }
}
