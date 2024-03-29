﻿namespace AlAnvar.ViewModels;

public partial class QuranViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    public QuranViewModel()
    {
        LoadQuranAsync();
        GetDefaultFont();
        GetDefaultForeground();
    }

    public void GetDefaultForeground()
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
    }

    public void GetDefaultFont()
    {
        AyatFontFamily = new FontFamily(Settings.AyatFontFamilyName);
        AyatNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
        TranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

        AyatFontSize = Settings.AyatFontSize;
        AyatNumberFontSize = Settings.AyatNumberFontSize;
        TranslationFontSize = Settings.TranslationFontSize;
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
            });
        });
        IsActive = false;
    }

    public void SearchSurah(AutoSuggestBox sender)
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

    public void AddNewSurahTab(TabView tabView, ChapterProperty chapterProperty, QuranSearch2 quranSearch2 = null)
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
        item.QuranSearch2 = quranSearch2;
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

    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SearchQuran(sender);
    }
    private void SearchQuran(AutoSuggestBox sender)
    {
        if (QuranPage.Instance != null)
        {
            int searchOptionIndex = 0;
            if (MainPage.Instance != null)
            {
                searchOptionIndex = MainPage.Instance.GetQuranSearchOptionIndex();
            }
            var tabView = QuranPage.Instance.GetTabView();
            var currentTabViewItem = tabView.TabItems?.Where(tabViewItem => (tabViewItem as QuranSearchItem)?.Tag?.ToString() == sender?.Text + searchOptionIndex.ToString())?.FirstOrDefault();
            if (currentTabViewItem is not null)
            {
                tabView.SelectedItem = currentTabViewItem;
                return;
            }

            var item = new QuranSearchItem();

            if (searchOptionIndex == 0)
            {
                item.Header = $"جستجو همه: {sender?.Text}";
            }
            else if (searchOptionIndex == 1)
            {
                item.Header = $"جستجو متن قرآن: {sender?.Text}";
            }
            else if (searchOptionIndex == 2)
            {
                item.Header = $"جستجو ترجمه قرآن: {sender?.Text}";
            }

            item.Tag = sender?.Text + searchOptionIndex.ToString();
            tabView.TabItems.Add(item);
            item.CloseRequested += TabViewItem_CloseRequested;
            tabView.SelectedIndex = tabView.TabItems.Count - 1;

            if (QuranTabViewItem.Instance != null)
            {
                QuranTabViewItem.Instance.viewModel.StatusText = "";
            }
        }
    }
}
