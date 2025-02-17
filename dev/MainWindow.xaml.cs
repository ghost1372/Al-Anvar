namespace AlAnvar;

public sealed partial class MainWindow : Window
{
    internal static MainWindow Instance { get; set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        var NavService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
        if (NavService != null)
        {
            NavService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                .ConfigureDefaultPage(typeof(HomeLandingPage))
                .ConfigureSettingsPage(typeof(SettingsPage))
                .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                .ConfigureAutoSuggestBox(HeaderAutoSuggestBox)
                .ConfigureTitleBar(AppTitleBar)
                .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
        }
    }
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeService.ChangeThemeWithoutSave(App.MainWindow);
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }

    private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        HeaderAutoSuggestBox.Focus(FocusState.Programmatic);
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSettingItem();
    }

    public void ActivateQuranSearchOption(bool isActive)
    {
        if (isActive)
        {
            cmbSearch.Visibility = Visibility.Visible;
        }
        else
        {
            cmbSearch.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 0: All
    /// 1: Quran
    /// 2: Translation
    /// </summary>
    /// <returns></returns>
    public int GetQuranSearchOptionIndex()
    {
        return rbSearch.SelectedIndex;
    }

    private void UpdateSettingItem()
    {
        var settings = (NavigationViewItem) NavView.SettingsItem;
        if (settings != null)
        {
            settings.Content = "تنظیمات";
        }
    }

    public void RefreshPaneDisplayMode()
    {
        NavView.PaneDisplayMode = Settings.PaneDisplayMode;
        UpdateSettingItem();
    }

    private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (NavView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
        {
            AppTitleBar.IsPaneToggleButtonVisible = false;
        }
        else
        {
            AppTitleBar.IsPaneToggleButtonVisible = true;
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
        UpdateSettingItem();
    }
}
