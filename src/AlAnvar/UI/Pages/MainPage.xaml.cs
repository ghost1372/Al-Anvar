namespace AlAnvar.UI.Pages;

public sealed partial class MainPage : Page
{
    internal static MainPage Instance { get; private set; }
    public MainPage()
    {
        this.InitializeComponent();
        Instance = this;
    }

    public void AddNewSurahTab(int surahId, string name, string type, int ayaCount)
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (tabViewItem as QuranTabViewItem)?.SurahId == surahId).FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{surahId} - {name} - {ayaCount} آیه - {type}";
        item.SurahId = surahId;
        item.SurahName = name;
        item.TotalAyah = ayaCount;
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
}
