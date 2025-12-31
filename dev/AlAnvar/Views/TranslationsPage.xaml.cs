using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class TranslationsPage : Page
{
    internal static TranslationsPage Instance { get; private set; }
    public string DownloadTag { get; } = "Download";

    public TranslationsPage()
    {
        InitializeComponent();
        Instance = this;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    public TableView GetTableView()
    {
        if (TranslationSelectorBar.SelectedItem is SelectorBarItem selectorBarItem)
        {
            return selectorBarItem.Tag.Equals(DownloadTag) ? DownloadTranslationsPage.Instance?.GetTableView() : LocalTranslationsPage.Instance?.GetTableView();
        }

        return null;
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (TranslationSelectorBar.SelectedItem is SelectorBarItem selectorBarItem)
        {
            DataContext = selectorBarItem.Tag.Equals(DownloadTag) ? DownloadTranslationsPage.Instance?.ViewModel : LocalTranslationsPage.Instance?.ViewModel;
        }
    }
}
