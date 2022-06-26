using System.Data;
using static AlAnvar.Common.ExplorerItem;

namespace AlAnvar.UI.Pages;

public sealed partial class ShellPage : Page
{
    public List<ExplorerItem> Subjects = new List<ExplorerItem>();
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

    public Type GetFrameContentType()
    {
        return shellFrame?.Content?.GetType();
    }

    public void Navigate(Type pageType, NavigationTransitionInfo transitionInfo = null, object parameter = null)
    {
        if (transitionInfo == null)
        {
            transitionInfo = new EntranceNavigationTransitionInfo();
        }

        if (pageType != typeof(MainPage))
        {
            DeSelectListView();
        }

        if (GetFrameContentType() != pageType)
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
            GetSubjects();
            DispatcherQueue.TryEnqueue(() =>
            {
                ChaptersACV = new AdvancedCollectionView(Chapters, true);
                ChaptersACV.SortDescriptions.Add(currentSortDescription);
                rootListView.ItemsSource = ChaptersACV;
                suggestListForSurahSearch = ChaptersACV.Select(x => ((ChapterProperty) x).Name).ToList();
                subjectTreeView.ItemsSource = Subjects;
            });
        });

        prgLoading.IsActive = false;
    }

    #region Quran TabViewItem
    public void DeSelectListView()
    {
        rootListView.SelectedIndex = -1;
    }
    public bool IsListViewItemSelected()
    {
        return rootListView.SelectedIndex > -1;
    }
    public void SetListViewItem(ChapterProperty chapterProperty)
    {
        rootListView.SelectedItem = chapterProperty;
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
            
            Navigate(typeof(MainPage));
            MainPage.Instance.AddNewSurahTab(selectedItem);
        }
    }
    #endregion

    #region Subject

    private async void GetSubjects()
    {
        using var db = new AlAnvarDBContext();
        var rootItems = await db.SubjectNames.Where(x=>x.ParentId == 0).ToListAsync();
        var subjectNames = await db.SubjectNames.ToListAsync();
        var subjects = await db.Subjects.ToListAsync();

        foreach (var root in rootItems)
        {
            var explorerItem = new ExplorerItem
            {
                Id = root.Id,
                Name = root.Name,
                Type = ExplorerItemType.Folder
            };
            
            var subjectNameChilds = subjectNames.Where(x => x.ParentId == root.SubjectId);

            if (subjectNameChilds.Any())
            {
                var childs = GetChilds(root, subjectNames, subjects, explorerItem);
                explorerItem.Children = new ObservableCollection<ExplorerItem>(childs);
            }
            Subjects.Add(explorerItem);
        }
    }
    public IEnumerable<ExplorerItem> GetChilds(SubjectName subjectName, List<SubjectName> subjectNameList, List<Subjects> subjectList, ExplorerItem parent)
    {
        var subjectNameChilds = subjectNameList.Where(x => x.ParentId == subjectName.SubjectId);

        foreach (var child in subjectNameChilds)
        {
            var explorerItem = new ExplorerItem
            {
                Id = child.Id,
                Name = child.Name,
                Parent = parent
            };
            if (child.Type == 0)
            {
                explorerItem.Type = ExplorerItemType.Folder;
            }
            else
            {
                var asdasd = subjectList.Where(x => x.SubjectId == child.SubjectId).Any();
                if (asdasd)
                {
                    explorerItem.Type = ExplorerItemType.CheckMark;
                }
                else
                {
                    explorerItem.Type = ExplorerItemType.File;
                }
            }

            var childs = subjectNameList.Where(x => x.ParentId == child.SubjectId);

            if (childs.Any())
            {
                var chils = GetChilds(child, subjectNameList, subjectList, explorerItem);
                explorerItem.Children = new ObservableCollection<ExplorerItem>(chils);
                yield return explorerItem;
            }
            else
            {
                yield return explorerItem;
            }
        }
    }

    private void TreeViewItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var treeViewItem = (sender as TreeViewItem).DataContext as ExplorerItem;
        Navigate(typeof(MainPage));
        MainPage.Instance.AddNewSubjectTab(treeViewItem);
    }
    #endregion
}
