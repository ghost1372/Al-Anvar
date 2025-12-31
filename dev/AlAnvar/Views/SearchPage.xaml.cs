using System.Text;
using AlAnvar.Models;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; }
    internal static SearchPage Instance { get; private set; }
    public SearchPage()
    {
        ViewModel = App.GetService<SearchViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
        Instance = this;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        QuranTableView.SortDescriptions.Add(item: new SortDescription("Id", SortDirection.Ascending));
        QuranTableView.SortDescriptions.Add(item: new SortDescription("AyaId", SortDirection.Ascending));

        QuranTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, ViewModel.Filter));

        await ViewModel.OnPageLoded();
    }

    public async void OnMenuClicked(object sender, RoutedEventArgs e)
    {
        var menu = sender as MenuFlyoutItem;
        if (menu != null && menu.Tag != null && menu.DataContext is QuranSearchModel quranSearch)
        {
            string suraStr = quranSearch.SuraId.ToString("D3"); // Format SuraId as XXX

            switch (menu.Tag.ToString())
            {
                case "CopyVerse":
                    CopyToClipboard(quranSearch.Aya);
                    break;
                case "CopyTranslation":
                    CopyToClipboard(quranSearch.Translation);
                    break;
                case "CopyAll":
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(quranSearch.Aya);
                    stringBuilder.AppendLine(quranSearch.Translation);

                    CopyToClipboard(stringBuilder.ToString());
                    break;
            }
        }
    }

    public TableView GetTableView()
    {
        return QuranTableView;
    }
}
