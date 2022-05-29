namespace AlAnvar.UI.Pages;

public sealed partial class ShellPage : Page
{
    public ObservableCollection<ChapterProperty> Chapters { get; set; }
    public AdvancedCollectionView ChaptersACV;

    private SortDescription currentSortDescription;
    private List<string> suggestListForSurahSearch = new List<string>();

    internal static ShellPage Instance { get; private set; }
    public ShellPage()
    {
        this.InitializeComponent();
        Instance = this;

        Loaded += ShellPage_Loaded;
    }

    public Frame GetFrame()
    {
        return shellFrame;
    }

    public void DeSelectListView()
    {
        rootListView.SelectedIndex = -1;
    }

    public void Navigate(Type pageType, NavigationTransitionInfo transitionInfo = null, object parameter = null)
    {
        if (transitionInfo == null)
        {
            transitionInfo = new EntranceNavigationTransitionInfo();
        }

        if (pageType != typeof(QuranPage))
        {
            DeSelectListView();
        }

        var currentType = shellFrame?.Content?.GetType();
        if (currentType != pageType)
        {
            shellFrame.Navigate(pageType, null, transitionInfo);
        }
    }
    private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();

            Chapters = new(await db.Chapters.ToListAsync());
            currentSortDescription = new SortDescription("Id", SortDirection.Ascending);
            DispatcherQueue.TryEnqueue(() =>
            {
                ChaptersACV = new AdvancedCollectionView(Chapters, true);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);
                rootListView.ItemsSource = ChaptersACV;
                suggestListForSurahSearch = ChaptersACV.Select(x => ((ChapterProperty) x).Name).ToList();
            });
        });
        prgLoading.IsActive = false;
    }

    private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ChaptersACV == null)
        {
            return;
        }
        ChaptersACV.SortDescriptions.Remove(currentSortDescription);
        switch (cmbSort.SelectedIndex)
        {
            case 0:
                currentSortDescription = new SortDescription("Id", SortDirection.Ascending);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);
                break;
            case 1:
                currentSortDescription = new SortDescription("Type", SortDirection.Descending);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);
                break;
            case 2:
                currentSortDescription = new SortDescription("Name", SortDirection.Ascending);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);

                break;
            case 3:
                currentSortDescription = new SortDescription("Ayas", SortDirection.Descending);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);

                break;
            case 4:
                currentSortDescription = new SortDescription("Ayas", SortDirection.Ascending);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);

                break;
        }
        ChaptersACV.RefreshSorting();
    }

    private void txtSurahSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.LoadSuggestions(sender, args, suggestListForSurahSearch, "نتیجه ای یافت نشد");
        ChaptersACV.Filter = _ => true;
        ChaptersACV.Filter = ChapterFilter;
    }
    private bool ChapterFilter(object chapter)
    {
        var query = chapter as ChapterProperty;

        var name = query.Name ?? "";
        var tName = query.TName ?? "";
        var type = query.Type ?? "";
        var aya = query.Ayas.ToString() ?? "";

        return name.Contains(txtSurahSearch.Text, StringComparison.OrdinalIgnoreCase)
                || tName.Contains(txtSurahSearch.Text, StringComparison.OrdinalIgnoreCase)
                || aya.Contains(txtSurahSearch.Text, StringComparison.OrdinalIgnoreCase)
                || type.Contains(txtSurahSearch.Text, StringComparison.OrdinalIgnoreCase);
    }

    private void rootListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (rootListView.SelectedIndex != -1)
        {
            var selectedItem = rootListView.SelectedItem as ChapterProperty;
            
            Navigate(typeof(QuranPage));
            QuranPage.Instance.AddNewTab(selectedItem.Id, selectedItem.Name, selectedItem.Type, selectedItem.Ayas);
        }
    }
}
