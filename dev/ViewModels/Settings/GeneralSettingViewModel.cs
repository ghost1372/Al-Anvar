namespace AlAnvar.ViewModels;

public partial class GeneralSettingViewModel : ObservableRecipient
{
    [RelayCommand]
    private void OnPaneDisplayModeChanged()
    {
        MainPage.Instance.RefreshPaneDisplayMode();
    }
}
