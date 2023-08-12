using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }
    public string AlAnvarVersion { get; set; } =
#if DEBUG
        $"v{App.Current.AlAnvarVersion} - Preview";
#else
        $"v{App.Current.AlAnvarVersion}";
#endif

    public static MainPage Instance { get; set; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        this.InitializeComponent();
        appTitleBar.Window = App.currentWindow;
        Instance = this;
        ViewModel.JsonNavigationViewService.Initialize(NavView, NavFrame);
        ViewModel.JsonNavigationViewService.ConfigJson("Assets/NavViewMenu/AppData.json");
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        var settings = (NavigationViewItem) NavView.SettingsItem;
        settings.Icon = new BitmapIcon { UriSource = new Uri("ms-appx:///Assets/Fluent/settings.png"), ShowAsMonochrome = false }; settings.Content = "Help";
        settings.Content = "تنظیمات";
    }

    private void TxtSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var rootFrame = ViewModel.JsonNavigationViewService.Frame;
        dynamic root = rootFrame.Content;
        dynamic viewModel = null;
        TxtSearch.ItemsSource = null;
        if (root is QariPage)
        {
            var frameContent = QariPage.Instance.GetFrame().Content;
            if (frameContent is DownloadQariPage)
            {
                viewModel = DownloadQariPage.Instance.ViewModel;
            }
        }
        if (viewModel != null)
        {
            viewModel.Search(sender, args);
        }
    }

    public AutoSuggestBox GetTxtSearch()
    {
        return TxtSearch;
    }

    private void appTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
    {
        if (NavFrame.CanGoBack)
        {
            NavFrame.GoBack();
        }
    }

    private void appTitleBar_PaneButtonClick(object sender, RoutedEventArgs e)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        appTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        var element = App.currentWindow.Content as FrameworkElement;

        if (element.ActualTheme == ElementTheme.Light)
        {
            element.RequestedTheme = ElementTheme.Dark;
        }
        else if (element.ActualTheme == ElementTheme.Dark)
        {
            element.RequestedTheme = ElementTheme.Light;
        }
    }
}
