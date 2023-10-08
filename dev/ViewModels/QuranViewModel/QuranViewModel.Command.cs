namespace AlAnvar.ViewModels;
public partial class QuranViewModel : ObservableRecipient
{
    [RelayCommand]
    private void OnPageLoaded()
    {

    }

    [RelayCommand]
    private void OnSortItemChanged(object sender)
    {
        var cmbSort = sender as ComboBox;
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

    [RelayCommand]
    private void OnListViewItemChanged(object sender)
    {
        var tabView = sender as TabView;
        if (ListViewSelectedIndex != -1)
        {
            var selectedItem = ListViewSelectedItem as ChapterProperty;
            AddNewSurahTab(tabView, selectedItem);
            StatusText = $"سوره: {selectedItem.Name}";
        }
    }

    [RelayCommand]
    private void OnTabViewItemChanged(object sender)
    {
        var tabView = sender as TabView;
        var quranTabViewItem = tabView.SelectedItem as QuranTabViewItem;
        if (quranTabViewItem is not null)
        {
            ListViewSelectedItem = quranTabViewItem.Chapter;
        }
        else
        {
            ListViewSelectedIndex = -1;
        }
    }

    [RelayCommand]
    private void OnChangeTabViewWidthMode(object sender)
    {
        var tabView = sender as TabView;
        tabView.TabWidthMode = tabView.TabWidthMode switch
        {
            TabViewWidthMode.Equal => TabViewWidthMode.SizeToContent,
            TabViewWidthMode.SizeToContent => TabViewWidthMode.Compact,
            TabViewWidthMode.Compact => TabViewWidthMode.Equal,
            _ => TabViewWidthMode.SizeToContent,
        };
    }
}
