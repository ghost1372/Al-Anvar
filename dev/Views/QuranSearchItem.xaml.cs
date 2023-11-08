using System.Collections.ObjectModel;

namespace AlAnvar.Views;

public sealed partial class QuranSearchItem : TabViewItem
{
    public ObservableCollection<QuranSearch2> quranSearches { get; set; }
    public QuranSearchItem()
    {
        this.InitializeComponent();
    }

    private async void TabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        quranSearches = new ObservableCollection<QuranSearch2>();
        prg.IsActive = true;
        listView.IsEnabled = false;
        infoBar.Title = "در حال جستجو...";
        infoBar.Message = "";

        int searchOptionIndex = 0;
        if (MainPage.Instance != null)
        {
            searchOptionIndex = MainPage.Instance.GetQuranSearchOptionIndex();
        }

        await SearchQuran(searchOptionIndex);

        if (quranSearches.Count > 0)
        {
            infoBar.Title = $"جستجو با موفقیت انجام شد. نتایج جستجو: {quranSearches.Count} مورد";
            infoBar.Message = " برای مشاهده متن کامل و هدایت به سوره موردنظر روی آیتم مورد نظر دوبار کلیک کنید یا روی آن راست کلیک کنید";
            infoBar.Severity = InfoBarSeverity.Success;
            listView.ItemsSource = quranSearches;
        }
        else
        {
            infoBar.Title = $"نتیجه ای یافت نشد";
            infoBar.Message = "";
            infoBar.Severity = InfoBarSeverity.Error;
        }

        listView.IsEnabled = true;
        prg.IsActive = false;
    }

    private async Task SearchQuran(int searchOptionIndex)
    {
        await Task.Run(() =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                using var db = new AlAnvarDBContext();
                List<QuranSearch2> items = new List<QuranSearch2>();
                int index = 1;
                var tag = Tag.ToString().Replace(searchOptionIndex.ToString(), "");

                if (searchOptionIndex == 0 || searchOptionIndex == 1)
                {
                    var quranResult = await db.QuranSearches.Where(x => x.AyahText.ToLower().Contains(tag.ToLower())).ToListAsync();
                    foreach (var item in quranResult)
                    {
                        var searchItem = new QuranSearch2
                        {
                            Id = index++,
                            SurahId = item.SurahId,
                            AyahNumber = item.AyahNumber,
                            Text = item.AyahText,
                            SurahName = db.Chapters.Where(x => x.Id == item.SurahId).Select(x => x.Name).FirstOrDefault(),
                            IsTranslation = false
                        };

                        items.Add(searchItem);
                    }
                }

                if (searchOptionIndex == 0 || searchOptionIndex == 2)
                {
                    if (Settings.QuranTranslation != null)
                    {
                        var translationItem = await db.TranslationsText.Where(x => x.TranslationId == Settings.QuranTranslation.TranslationId).FirstOrDefaultAsync();
                        if (translationItem != null)
                        {
                            var translations = System.Text.Json.JsonSerializer.Deserialize<List<TranslationItem>>(translationItem.Text);
                            var translationResult = translations.Where(x => x.Translation.ToLower().Contains(tag.ToLower())).ToList();
                            foreach (var item in translationResult)
                            {
                                var searchItem = new QuranSearch2
                                {
                                    Id = index++,
                                    SurahId = item.SurahId,
                                    AyahNumber = item.Aya,
                                    Text = item.Translation,
                                    SurahName = db.Chapters.Where(x => x.Id == item.SurahId).Select(x => x.Name).FirstOrDefault(),
                                    IsTranslation = true
                                };

                                items.Add(searchItem);
                            }
                        }
                    }
                }

                quranSearches = new(items);
            });
        });
    }

    private void MenuGoToSurah_Click(object sender, RoutedEventArgs e)
    {
        var menu = sender as MenuFlyoutItem;
        if (menu != null && menu.Tag != null)
        {
            GoToSurah(menu.Tag as QuranSearch2);
        }
    }

    private void DataRow_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var dataRow = sender as DataRow;
        if (dataRow != null && dataRow.Tag != null)
        {
            GoToSurah(dataRow.Tag as QuranSearch2);
        }
    }

    private async void GoToSurah(QuranSearch2 quranSearch2)
    {
        if (quranSearch2 != null)
        {
            if (QuranPage.Instance != null)
            {
                using var db = new AlAnvarDBContext();
                var chapter = await db.Chapters.Where(x=> x.Name == quranSearch2.SurahName).FirstOrDefaultAsync();
                if (chapter != null)
                {
                    QuranPage.Instance.ViewModel.AddNewSurahTab(QuranPage.Instance.GetTabView(), chapter, quranSearch2);
                }
            }
        }
    }

    private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (listView.SelectedItem != null)
        {
            listView.Focus(Settings.FocusState);
        }
    }
}
