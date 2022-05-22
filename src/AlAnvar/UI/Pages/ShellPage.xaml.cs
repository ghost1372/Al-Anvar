﻿namespace AlAnvar.UI.Pages;

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
    private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        using var db = new AlAnvarDBContext();

        Chapters = new(await db.Chapters.ToListAsync());
        ChaptersACV = new AdvancedCollectionView(Chapters, true);
        currentSortDescription = new SortDescription("Id", SortDirection.Ascending);
        ChaptersACV.SortDescriptions.Add(currentSortDescription);
        rootListView.ItemsSource = ChaptersACV;
        suggestListForSurahSearch = ChaptersACV.Select(x => ((ChapterProperty)x).Name).ToList();
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
        var currentType = shellFrame?.Content?.GetType();
        var selectedItem = rootListView.SelectedItem as ChapterProperty;
        if (currentType != typeof(QuranPage))
        {
            shellFrame.Navigate(typeof(QuranPage), null, new EntranceNavigationTransitionInfo());
            QuranPage.Instance.AddNewTab(selectedItem.Id, selectedItem.Name, selectedItem.Ayas);
        }
        else
        {
            QuranPage.Instance.AddNewTab(selectedItem.Id, selectedItem.Name, selectedItem.Ayas);
        }
    }
}