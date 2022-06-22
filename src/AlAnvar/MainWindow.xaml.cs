namespace AlAnvar;

public sealed partial class MainWindow : Window
{
    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        SetDefaultFoldersPath();
        if (OSVersionHelper.IsWindows10_1809 && !OSVersionHelper.IsWindows11_OrGreater)
        {
            mainGrid.Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush;
        }

        TitleBarHelper.Initialize(this, TitleTextBlock, AppTitleBar, LeftPaddingColumn, IconColumn, TitleColumn, LeftDragColumn, SearchColumn, RightDragColumn, RightPaddingColumn);
    }
    public void SetDefaultFoldersPath()
    {
        Settings.TranslationsPath = Path.Combine(Settings.AppFolderPath, Constants.TranslationsFolderName);
        Settings.AudiosPath = Path.Combine(Settings.AppFolderPath, Constants.AudiosFolderName);
    }

    public void SetMainGridFlowDirection(FlowDirection flowDirection)
    {
        mainGrid.FlowDirection = flowDirection;
    }
    private async void MenuBarItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string pageName })
        {
            switch (pageName)
            {
                case "TafsirPage":
                    ShellPage.Instance.Navigate(typeof(MainPage));
                    MainPage.Instance.AddNewTafsirTab();
                    break;
                case "SettingsPage":
                    ShellPage.Instance.Navigate(typeof(SettingsPage));
                    break;
                case "TranslationPage":
                    ShellPage.Instance.Navigate(typeof(TranslationPage));
                    break;
                case "QariPage":
                    ShellPage.Instance.Navigate(typeof(QariPage));
                    break;
                case "GoToGithub":
                    await Launcher.LaunchUriAsync(new Uri(Constants.AppGithubPage));
                    break;
                case "AboutDialog":
                    aboutContentDialog.XamlRoot = Content.XamlRoot;
                    await aboutContentDialog.ShowAsyncQueue();
                    break;
                case "Print":
                    if (MainPage.Instance is null || !ShellPage.Instance.GetFrameContentType().Equals(typeof(MainPage)))
                        return;

                    var tabItem = MainPage.Instance.GetTabViewItem();

                    if (tabItem is null)
                        return;

                    if (tabItem.GetType() == typeof(QuranTabViewItem))
                    {
                        QuranTabViewItem.Instance.ShowPrintDialog();
                    }
                    else if (tabItem.GetType() == typeof(TafsirTabViewItem))
                    {
                        TafsirTabViewItem.Instance.Print();
                    }

                    break;
                case "Exit":
                    Application.Current.Exit();
                    break;
            }
        }
    }
}
