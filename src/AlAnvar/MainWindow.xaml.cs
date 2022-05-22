using Microsoft.UI.Xaml.Input;

namespace AlAnvar;

public sealed partial class MainWindow : Window
{
    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        TitleBarHelper.Initialize(this, TitleTextBlock, AppTitleBar, LeftPaddingColumn, IconColumn, TitleColumn, LeftDragColumn, SearchColumn, RightDragColumn, RightPaddingColumn);
    }

    private void MenuBarItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string pageName })
        {

            switch (pageName)
            {
                case "SettingsPage":
                    ShellPage.Instance.GetFrame().Navigate(typeof(SettingsPage), null, new EntranceNavigationTransitionInfo());
                    break;
                case "TranslationPage":
                    ShellPage.Instance.GetFrame().Navigate(typeof(TranslationPage), null, new EntranceNavigationTransitionInfo());
                    break;
                case "Exit":
                    Application.Current.Exit();
                    break;
                default:
                    break;
            }
        }
    }
}
