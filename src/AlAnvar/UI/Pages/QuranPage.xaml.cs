namespace AlAnvar.UI.Pages;

public sealed partial class QuranPage : Page
{
    internal static QuranPage Instance { get; private set; }
    public QuranPage()
    {
        this.InitializeComponent();
        Instance = this;
        LoadTranslationsInCombobox();
    }

    public void AddNewTab(int surahId, string name, string type, int ayaCount)
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (tabViewItem as QuranTabViewItem).SurahId == surahId).FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{surahId} - {name} - {ayaCount} آیه - {type}";
        item.SurahId = surahId;
        item.TotalAyah = ayaCount;
        tabView.TabItems.Add(item);
        item.CloseRequested += Item_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }

    private void Item_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        tabView.TabItems.Remove(sender);
    }

    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuranTabViewItem.Instance is not null)
        {
            Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;
            var itemIndex = QuranTabViewItem.Instance.GetCurrentListViewItemIndex();
            QuranTabViewItem.Instance.GetTranslationText();
            QuranTabViewItem.Instance.GetSuraText();
            QuranTabViewItem.Instance.ScrollIntoView(itemIndex);
        }
    }

    public QuranTranslation GetComboboxFirstElement()
    {
        cmbTranslators.SelectedIndex = 0;
        return cmbTranslators.SelectedItem as QuranTranslation;
    }

    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Constants.TranslationsPath))
        {
            var items = new ObservableCollection<QuranTranslation>();
            var files = Directory.GetFiles(Constants.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(file));
                    if (trans is not null)
                    {
                        items.Add(trans);
                    }
                }
                cmbTranslators.ItemsSource = items;
                cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId).FirstOrDefault();
            }
        }
    }

    private void chkOnlyAyaText_Checked(object sender, RoutedEventArgs e)
    {
        if (QuranTabViewItem.Instance is not null)
        {
            QuranTabViewItem.Instance.IsSurahTextAvailable = chkOnlyAyaText.IsChecked.Value;
        }
    }

    private void chkOnlyTranslationText_Checked(object sender, RoutedEventArgs e)
    {
        if (QuranTabViewItem.Instance is not null)
        {
            QuranTabViewItem.Instance.IsTranslationAvailable = chkOnlyTranslationText.IsChecked.Value;
        }
    }
}
