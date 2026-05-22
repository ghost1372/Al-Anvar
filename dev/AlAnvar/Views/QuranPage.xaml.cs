using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class QuranPage : Page
{
    public QuranViewModel ViewModel { get; }
    private CancellationTokenSource? _token;
    internal static QuranPage Instance { get; set; }
    private bool canNavigateToVerse;
    private int verseSelectedIndex = -1; 
    public QuranPage()
    {
        ViewModel = App.GetService<QuranViewModel>();
        InitializeComponent();
        Instance = this;

        DataContext = ViewModel;

        MetaTableView.SortDescriptions.Add(new SortDescription("Id", SortDirection.Ascending));
        MetaTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, Filter));

        var verseAlignment = ViewModel.VerseTextAlignment;
        var verseBarItem = VerseSelectorBar.Items.OfType<SelectorBarItem>().Where(x => GeneralHelper.GetEnum<TextAlignment>(x.Tag.ToString()) == verseAlignment).FirstOrDefault();
        VerseSelectorBar.SelectedItem = verseBarItem;

        var translationAlignment = ViewModel.TranslationTextAlignment;
        var translationBarItem = TranslationSelectorBar.Items.OfType<SelectorBarItem>().Where(x => GeneralHelper.GetEnum<TextAlignment>(x.Tag.ToString()) == translationAlignment).FirstOrDefault();
        TranslationSelectorBar.SelectedItem = translationBarItem;
    }
    
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ShowDialogIfAudioOrTranslationNotAvailable();

        if (await EnsureDatabaseExistsAsync())
        {
            await ViewModel.OnPageLoaded();
        }

        var quranFont = CmbQuranFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.QuranFont.FontKey).FirstOrDefault();
        var quranFontSize = CmbQuranFontSize.Items.OfType<double>().Where(x => x == Constants.DefaultQuranFontSize).FirstOrDefault();
        var translationFont = CmbTranslationFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.TranslationFont.FontKey).FirstOrDefault();
        var translationFontSize = CmbTranslationFontSize.Items.OfType<double>().Where(x => x == Constants.DefaultTranslationFontSize).FirstOrDefault();
        CmbQuranFont.SelectedItem = quranFont;
        CmbQuranFontSize.SelectedItem = quranFontSize;
        CmbTranslationFont.SelectedItem = translationFont;
        CmbTranslationFontSize.SelectedItem = translationFontSize;
    }

    private void GoToAyaNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (MainTabView.SelectedItem is QuranTabViewItem tabViewItem)
        {
            tabViewItem.GoToAya((int)args.NewValue);
        }
    }

    private bool Filter(object? item)
    {
        if (string.IsNullOrWhiteSpace(TxtMetaSearch.Text)) return true;
        if (item is null) return false;

        var model = (QuranMetadataTable)item;

        return model.Name?.Contains(TxtMetaSearch.Text, StringComparison.OrdinalIgnoreCase) is true ||
               model.FinglishName?.Contains(TxtMetaSearch.Text, StringComparison.OrdinalIgnoreCase) is true ||
               model.EnglishName?.Contains(TxtMetaSearch.Text, StringComparison.OrdinalIgnoreCase) is true ||
               model.Aya.ToString()?.Contains(TxtMetaSearch.Text, StringComparison.OrdinalIgnoreCase) is true ||
               model.Type?.Contains(TxtMetaSearch.Text, StringComparison.OrdinalIgnoreCase) is true;
    }

    private async void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (_token is not null)
        {
            _token.Cancel();
        }

        _token = new CancellationTokenSource();
        await RefreshFilter(_token.Token);
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
        MetaTableView.RefreshFilter();
    }

    private void OnAutoPlayNextToggleButtonChecked(object sender, RoutedEventArgs e)
    {
        var tg = sender as AppBarToggleButton;
        if (tg != null)
        {
            tg.Icon = tg.IsChecked.Value ? new FontIcon() { Glyph = "\uEC57" } : new FontIcon() { Glyph = "\uE8ED" };
        }
    }
    private void OnToggleButtonChecked(object sender, RoutedEventArgs e)
    {
        var tg = sender as AppBarToggleButton;
        if (tg != null)
        {
            tg.Icon = tg.IsChecked.Value ? new FontIcon() { Glyph = "\uE7B3" } : new FontIcon() { Glyph = "\uED1A" };
        }
    }

    public void AddNewSurahTab(QuranMetadataTable metaData)
    {
        var currentTabViewItem = MainTabView.TabItems?.OfType<QuranTabViewItem>().Where(tabViewItem => tabViewItem?.Metadata?.Id == metaData.Id)?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            MainTabView.SelectedItem = currentTabViewItem;
            currentTabViewItem.GoToAya(verseSelectedIndex);
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{metaData.Id} - {metaData.Name} - {metaData.Aya} {Strings.QuranPage_Aya.GetLocalizedResource()}";
        item.Metadata = metaData;
        item.QuranViewModel = ViewModel;
        if (canNavigateToVerse)
        {
            item.VerseSelectedIndex = verseSelectedIndex;
        }

        canNavigateToVerse = false;
        verseSelectedIndex = -1;

        MainTabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        MainTabView.SelectedIndex = MainTabView.TabItems.Count - 1;
    }

    public void AddNewFavoriteTab()
    {
        var currentTabViewItem = MainTabView.TabItems?.OfType<FavoriteTabViewItem>()?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            MainTabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new FavoriteTabViewItem();
        item.QuranViewModel = ViewModel;
        item.Metadata = ViewModel.QuranMetaItems;
        item.Header = Strings.QuranPage_FavoriteTabViewItem.GetLocalizedResource();
        MainTabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        MainTabView.SelectedIndex = MainTabView.TabItems.Count - 1;
    }
    public void AddNewTafsirTab(FinalQuran finalQuran, string tafsirName, TafsirType tafsirType)
    {
        var currentTabViewItem = MainTabView.TabItems?.OfType<TafsirTabViewItem>().Where(tabViewItem => tabViewItem?.FinalQuran?.SuraId == finalQuran.SuraId && tabViewItem?.FinalQuran?.AyaId == finalQuran.AyaId && tabViewItem.TafsirType == tafsirType)?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            MainTabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new TafsirTabViewItem();
        item.FinalQuran = finalQuran;
        item.TafsirType = tafsirType;
        item.Header = string.Format(Strings.QuranPage_TafsirTabViewItem.GetLocalizedResource(), tafsirName, finalQuran.SuraFinglishName, finalQuran.AyaId);
        MainTabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        MainTabView.SelectedIndex = MainTabView.TabItems.Count - 1;
    }
    private void TabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        MainTabView.TabItems.Remove(sender);

        if (MainTabView.SelectedItem is QuranTabViewItem selectedTab && selectedTab.Metadata != null)
        {
            var item = MetaTableView.Items.OfType<QuranMetadataTable>().FirstOrDefault(x => x.Id == selectedTab.Metadata.Id);

            if (item != null)
            {
                MetaTableView.SelectedItem = item;
                return;
            }
        }

        MetaTableView.SelectedIndex = -1;
    }

    private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabView.SelectedItem is QuranTabViewItem selectedTab && selectedTab.Metadata != null)
        {
            var item = MetaTableView.Items.OfType<QuranMetadataTable>().FirstOrDefault(x => x.Id == selectedTab.Metadata.Id);

            if (item != null && MetaTableView.SelectedItem != item)
            {
                MetaTableView.SelectedItem = item;
            }
        }

        ViewModel.HasTabItems = MainTabView.TabItems.Count > 0;
    }

    private void CmbAudio_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuranTabViewItem.Instance == null)
            return;
    }

    private void MetaTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MetaTableView.SelectedIndex != -1)
        {
            if (MetaTableView.SelectedItem is QuranMetadataTable selectedItem)
            {
                AddNewSurahTab(selectedItem);
            }
        }
    }

    public void GoToVerse(int suraId, int verseId)
    {
        canNavigateToVerse = true;
        verseSelectedIndex = verseId;
        var item = MetaTableView.Items.OfType<QuranMetadataTable>().Where(x=>x.Id == suraId).FirstOrDefault();
        if (MetaTableView.SelectedItem is QuranMetadataTable quranMetadataTable && quranMetadataTable.Equals(item))
        {
            AddNewSurahTab(quranMetadataTable);
        }
        else
        {
            MetaTableView.SelectedItem = item;
        }
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        foreach (var item in MainTabView.TabItems.OfType<QuranTabViewItem>())
        {
            item.DisposeAudio();
        }
    }

    private void OnVerseSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem selectorBarItem)
        {
            ViewModel.VerseTextAlignment = GeneralHelper.GetEnum<TextAlignment>(selectorBarItem.Tag.ToString());
        }
    }
    private void OnTranslationSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem selectorBarItem)
        {
            ViewModel.TranslationTextAlignment = GeneralHelper.GetEnum<TextAlignment>(selectorBarItem.Tag.ToString());
        }
    }

    private void AudioGoToCard_ActionClick(object sender, RoutedEventArgs e)
    {
        EnsureNavigationSelection(typeof(AudiosPage));
    }

    private void TranslationGoToCard_ActionClick(object sender, RoutedEventArgs e)
    {
        EnsureNavigationSelection(typeof(TranslationsPage));
    }

    private async void OnQuranFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbQuranFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.QuranFont.FontKey)
                return;

            Settings.QuranFont = font;
            FontHelper.SetQuranFontFamily(font);
        }
    }
    private async void OnQuranFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbQuranFontSize.SelectedItem is double fontSize)
        {
            ProxyService.Instance.QuranFontSize = fontSize;
        }
    }
    private async void OnTranslationFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTranslationFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.TranslationFont.FontKey)
                return;
            Settings.TranslationFont = font;
            FontHelper.SetTranslationFontFamily(font);
        }
    }

    private void CmbTranslation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var item in MainTabView.TabItems)
        {
            switch (item)
            {
                case QuranTabViewItem q:
                    q.Refresh();
                    break;

                case FavoriteTabViewItem f:
                    f.Refresh();
                    break;
            }
        }
    }

    private async void OnTranslationFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTranslationFontSize.SelectedItem is double fontSize)
        {
            ProxyService.Instance.TranslationFontSize = fontSize;
        }
    }

    private void OnQuranColorChanged(object sender, DropdownColorPickerColorChangedEventArgs e)
    {
        ProxyService.Instance.QuranColor = new Microsoft.UI.Xaml.Media.SolidColorBrush(e.Color);
    }
    private void OnTranslationColorChanged(object sender, DropdownColorPickerColorChangedEventArgs e)
    {
        ProxyService.Instance.TranslationColor = new Microsoft.UI.Xaml.Media.SolidColorBrush(e.Color);
    }
    private void OnQuranNumberColorChanged(object sender, DropdownColorPickerColorChangedEventArgs e)
    {
        ProxyService.Instance.QuranNumberColor = new Microsoft.UI.Xaml.Media.SolidColorBrush(e.Color);
    }

    private void BtnResetColors_Click(object sender, RoutedEventArgs e)
    {
        DCPQuran.Color = Colors.Transparent;
        DCPTranslation.Color = Colors.Transparent;
        DCPQuranNumber.Color = Colors.Transparent;
    }

    private void BtnGoToFav_Click(object sender, RoutedEventArgs e)
    {
        AddNewFavoriteTab();
    }

    private void BtnGoToNote_Click(object sender, RoutedEventArgs e)
    {
        EnsureNavigationSelection(typeof(NotePage));
    }

    private async void BtnAddNote_Click(object sender, RoutedEventArgs e)
    {
        if (!await EnsureDatabaseExistsAsync())
            return;

        var dialog = new AddNoteDialog(ViewModel);
        if (dialog != null)
        {
            ViewModel.CanSaveNote = false;
            ViewModel.TitleNote = string.Empty;
            ViewModel.DescriptionNote = string.Empty;

            dialog.ShowDialog();
        }
    }

    public TableView GetTableView()
    {
        if (MainTabView.SelectedItem is QuranTabViewItem quranTabViewItem)
        {
            return quranTabViewItem.GetTableView();
        }
        if (MainTabView.SelectedItem is FavoriteTabViewItem favoriteTabViewItem)
        {
            return favoriteTabViewItem.GetTableView();
        }

        return null;
    }

    private void ShowDialogIfAudioOrTranslationNotAvailable()
    {
        if (Settings.IsFirstRun && (Settings.Translation == null || Settings.Audio == null))
        {
            Settings.IsFirstRun = false;

            var dialog = new WindowedContentDialog()
            {
                Header = Strings.QuranPage_MediaNotFoundTitle.GetLocalizedResource(),
                PrimaryButtonContent = Strings.QuranPage_MediaNotFoundPrimaryButtonText.GetLocalizedResource(),
                CloseButtonContent = Strings.QuranPage_MediaNotFoundCloseButtonText.GetLocalizedResource(),
                DefaultButton = ContentDialogButton.Primary,
                Content = Strings.QuranPage_MediaNotFound.GetLocalizedResource(),
                Owner = App.MainWindow,
                HasTitleBar = false,
                MinWidth = 500,
                FlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource())
            };

            dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            dialog.PrimaryButtonClick += OnPrimaryButtonClick;

            dialog.ShowAsync();

            void OnPrimaryButtonClick(object sender, EventArgs args)
            {
                App.Current.NavService.NavigateTo(typeof(SettingsPage));
            }
        }
    }
}
