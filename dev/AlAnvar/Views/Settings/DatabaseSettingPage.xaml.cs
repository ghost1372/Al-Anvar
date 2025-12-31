namespace AlAnvar.Views;

public sealed partial class DatabaseSettingPage : Page
{
    public DatabaseSettingViewModel ViewModel { get; }
    public DatabaseSettingPage()
    {
        ViewModel = App.GetService<DatabaseSettingViewModel>();
        InitializeComponent();
    }

    private void OnBackup(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveFileAsync();
    }
    private void OnRestore(object sender, RoutedEventArgs e)
    {
        ViewModel.RestoreFileAsync();
    }
}
