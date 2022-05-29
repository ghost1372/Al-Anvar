namespace AlAnvar.UI.Pages;

public sealed partial class MainPage : Page
{
    internal static MainPage Instance { get; private set; }
    public MainPage()
    {
        this.InitializeComponent();
        Instance = this;
    }

    public void AddNewSurahTab(ChapterProperty chapterProperty)
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (tabViewItem as QuranTabViewItem)?.SurahId == chapterProperty.Id).FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{chapterProperty.Id} - {chapterProperty.Name} - {chapterProperty.Ayas} آیه - {chapterProperty.Type}";
        item.SurahId = chapterProperty.Id;
        item.SurahName = chapterProperty.Name;
        item.TotalAyah = chapterProperty.Ayas;
        item.ChapterInstance = chapterProperty;
        tabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }

    public void AddNewTafsirTab()
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (string)(tabViewItem as TafsirTabViewItem)?.Header == Constants.TafsirTabViewItemHeader).FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new TafsirTabViewItem();
        item.Header = Constants.TafsirTabViewItemHeader;
        tabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }
    private void TabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        tabView.TabItems.Remove(sender);
        ShellPage.Instance.DeSelectListView();
    }
    public TabView GetTabView()
    {
        return tabView;
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var quranTabViewItem = tabView.SelectedItem as QuranTabViewItem;
        if (quranTabViewItem is not null)
        {
            ShellPage.Instance.SetListViewItem(quranTabViewItem.ChapterInstance);
        }
        else
        {
            ShellPage.Instance.DeSelectListView();
        }
    }
}
