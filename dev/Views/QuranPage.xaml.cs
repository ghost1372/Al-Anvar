using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class QuranPage : Page
{
    public static QuranPage Instance { get; set; }
    public QuranViewModel ViewModel { get; }
    public QuranPage()
    {
        ViewModel = App.GetService<QuranViewModel>();
        this.InitializeComponent();
        Instance = this;
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.ActivateQuranSearchOption(true);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.ActivateQuranSearchOption(false);
        }
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.ActivateQuranSearchOption(true);
        }
    }

    public TabView GetTabView()
    {
        return tabView;
    }

    public QuranTabViewItem GetTabViewItem()
    {
        return tabView.SelectedItem as QuranTabViewItem;
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (tabView.TabItems.Count > 0)
        {
            LogoImage.Visibility = Visibility.Collapsed;
        }
        else
        {
            LogoImage.Visibility = Visibility.Visible;
        }
    }

    private void txtSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        ViewModel.SearchSurah(sender);
    }

    private void txtSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchSurah(sender);
    }
}
