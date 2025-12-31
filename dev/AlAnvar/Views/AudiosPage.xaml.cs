using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class AudiosPage : Page
{
    public static AudiosPage Instance { get; private set; }
    public string DownloadTag { get; } = "Download";
    public AudiosPage()
    {
        InitializeComponent();
        Instance = this;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    public TableView GetTableView()
    {
        if (AudioSelectorBar.SelectedItem is SelectorBarItem selectorBarItem)
        {
            return selectorBarItem.Tag.Equals(DownloadTag) ? DownloadAudiosPage.Instance?.GetTableView() : LocalAudiosPage.Instance?.GetTableView();
        }

        return null;
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AudioSelectorBar.SelectedItem is SelectorBarItem selectorBarItem)
        {
            DataContext = selectorBarItem.Tag.Equals(DownloadTag) ? DownloadAudiosPage.Instance?.ViewModel : LocalAudiosPage.Instance?.ViewModel;
        }
    }
}
