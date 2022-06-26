using AlAnvar.Database.Tables;

namespace AlAnvar.UI.Pages;

public sealed partial class MainPage : Page
{
    internal static MainPage Instance { get; private set; }
    private ShellPage shellPage = ShellPage.Instance;
    public MainPage()
    {
        this.InitializeComponent();
        Instance = this;
    }

    public void AddNewSurahTab(ChapterProperty chapterProperty)
    {
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

    public void AddNewTafsirTab()
    {
        var currentTabViewItem = tabView.TabItems?.Where(tabViewItem => (string) (tabViewItem as TafsirTabViewItem)?.Header == Constants.TafsirTabViewItemHeader)?.FirstOrDefault();
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
    public void AddNewSingleTafsirTab(QuranItem quranItem, ChapterProperty chapterProperty)
    {
        var currentTabViewItem = tabView.TabItems?.Where(tabViewItem => (string) (tabViewItem as SingleTafsirTabViewItem)?.Header == $"تفسیر سوره: {quranItem.SurahName} - آیه: {quranItem.AyahNumber}")?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new SingleTafsirTabViewItem();
        item.Header = $"تفسیر سوره: {quranItem.SurahName} - آیه: {quranItem.AyahNumber}";
        item.QuranItem = quranItem;
        item.Chapter = chapterProperty;
        tabView.TabItems.Add(item);
        item.CloseRequested += TabViewItem_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }

    public void AddNewSubjectTab(ExplorerItem explorerItem)
    {
        var currentTabViewItem = tabView.TabItems?.Where(tabViewItem => (tabViewItem as SubjectTabViewItem)?.Subject?.Id == explorerItem.Id)?.FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var header = $"{explorerItem.Parent.Name} : {explorerItem.Name}";
        var item = new SubjectTabViewItem();
        item.Header = header;
        item.Subject = explorerItem;
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

    public TabViewItem GetTabViewItem()
    {
        return (TabViewItem) tabView.SelectedItem;
    }
    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var quranTabViewItem = tabView.SelectedItem as QuranTabViewItem;
        if (quranTabViewItem is not null)
        {
            shellPage.SetListViewItem(quranTabViewItem.Chapter);
        }
        else
        {
            shellPage.DeSelectListView();
        }
    }

    private void btnChangeTabViewWidthMode_Click(object sender, RoutedEventArgs e)
    {
        tabView.TabWidthMode = tabView.TabWidthMode switch
        {
            TabViewWidthMode.Equal => TabViewWidthMode.SizeToContent,
            TabViewWidthMode.SizeToContent => TabViewWidthMode.Compact,
            TabViewWidthMode.Compact => TabViewWidthMode.Equal,
            _ => TabViewWidthMode.SizeToContent,
        };
    }
}
