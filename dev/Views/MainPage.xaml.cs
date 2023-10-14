using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }
    public string AlAnvarVersion { get; set; } =
#if DEBUG
        $"v{App.Current.AppVersion} - Preview";
#else
        $"v{App.Current.AppVersion}";
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

    public void RefreshPaneDisplayMode()
    {
        NavView.PaneDisplayMode = Settings.PaneDisplayMode;
    }

    private void Search(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs textChangedEventArgs, AutoSuggestBoxQuerySubmittedEventArgs querySubmittedEventArgs)
    {
        var rootFrame = ViewModel.JsonNavigationViewService.Frame;
        dynamic root = rootFrame.Content;
        dynamic viewModel = null;
        if (root is QariPage)
        {
            viewModel = QariPage.Instance.GetFrame().GetPageViewModel();
        }
        else if (root is TranslationPage)
        {
            viewModel = TranslationPage.Instance.GetFrame().GetPageViewModel();
        }

        if (viewModel != null && viewModel is ITitleBarAutoSuggestBoxAware)
        {
            var titleBarAutoSuggestBoxAware = viewModel as ITitleBarAutoSuggestBoxAware;
            if (textChangedEventArgs != null)
            {
                titleBarAutoSuggestBoxAware.OnAutoSuggestBoxTextChanged(sender, textChangedEventArgs);
            }
            else
            {
                titleBarAutoSuggestBoxAware.OnAutoSuggestBoxQuerySubmitted(sender, querySubmittedEventArgs);
            }
        }
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var viewModel = NavFrame.GetPageViewModel();
        if (viewModel != null && viewModel is ITitleBarAutoSuggestBoxAware titleBarAutoSuggestBoxAware)
        {
            titleBarAutoSuggestBoxAware.OnAutoSuggestBoxTextChanged(sender, args);
        }

        Search(sender, args, null);
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var viewModel = NavFrame.GetPageViewModel();
        if (viewModel != null && viewModel is ITitleBarAutoSuggestBoxAware titleBarAutoSuggestBoxAware)
        {
            titleBarAutoSuggestBoxAware.OnAutoSuggestBoxQuerySubmitted(sender, args);
        }

        Search(sender, null, args);
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

    private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (NavView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
        {
            appTitleBar.IsPaneButtonVisible = false;
        }
        else
        {
            appTitleBar.IsPaneButtonVisible = true;
        }
    }

    private void PaneDisplayModeButton_Click(object sender, RoutedEventArgs e)
    {
        switch (NavView.PaneDisplayMode)
        {
            case NavigationViewPaneDisplayMode.Auto:
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                break;
            case NavigationViewPaneDisplayMode.Left:
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                break;
            case NavigationViewPaneDisplayMode.Top:
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                break;
            case NavigationViewPaneDisplayMode.LeftCompact:
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                break;
            case NavigationViewPaneDisplayMode.LeftMinimal:
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                break;
        }

        Settings.PaneDisplayMode = NavView.PaneDisplayMode;
    }
}
