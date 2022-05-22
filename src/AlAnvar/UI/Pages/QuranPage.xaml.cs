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

    public void AddNewTab(int surahId, string name, int ayaCount)
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (tabViewItem as QuranTabViewItem).SurahId == surahId).FirstOrDefault();
        if (currentTabViewItem != null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = name;
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
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;
        var itemIndex = QuranTabViewItem.Instance.GetCurrentListViewItemIndex();
        QuranTabViewItem.Instance.GetTranslationText();
        QuranTabViewItem.Instance.GetSuraText();
        QuranTabViewItem.Instance.ScrollIntoView(itemIndex);
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
                    if (trans != null)
                    {
                        items.Add(trans);
                    }
                }
                cmbTranslators.ItemsSource = items;
            }
        }
    }
}
