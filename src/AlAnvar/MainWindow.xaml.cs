namespace AlAnvar;

public sealed partial class MainWindow : Window
{
    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        SetDefaultFoldersPath();
        if (OSVersionHelper.IsWindows10_OrGreater && !OSVersionHelper.IsWindows11_OrGreater)
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
    private async void MenuBarItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string pageName })
        {
            switch (pageName)
            {
                case "TafsirPage":
                    
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
                case "Exit":
                    Application.Current.Exit();
                    break;
            }
        }
    }
}
