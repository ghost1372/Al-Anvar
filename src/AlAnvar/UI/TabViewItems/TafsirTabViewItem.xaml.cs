namespace AlAnvar.UI.TabViewItems;
public sealed partial class TafsirTabViewItem : TabViewItem
{
    public TafsirTabViewItem()
    {
        this.InitializeComponent();
        Loaded += TafsirTabViewItem_Loaded;
    }

    private async void TafsirTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        tafsirTreeView.ItemsSource = await GetData();
        prgLoading.IsActive = false;
    }
    private async Task<ObservableCollection<ExplorerItem>> GetData()
    {
        using var db = new AlAnvarDBContext();
        var chapters = await db.Chapters.ToListAsync();
        var list = new ObservableCollection<ExplorerItem>();

        foreach (var item in chapters)
        {
            ExplorerItem rootNode = new ExplorerItem() { Name = item.Name, Type = ExplorerItem.ExplorerItemType.Folder };
            rootNode.IsExpanded = true;
            for (int i = 1; i <= item.Ayas; i++)
            {
                rootNode.Children.Add(new ExplorerItem()
                {
                    Name = $"آیه: {i}",
                    Type = ExplorerItem.ExplorerItemType.File,
                });
            }
            list.Add(rootNode);
        }
        return list;
    }
}
