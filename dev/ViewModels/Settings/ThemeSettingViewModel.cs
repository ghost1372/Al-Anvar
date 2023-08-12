using Windows.System;

namespace AlAnvar.ViewModels;

public partial class ThemeSettingViewModel : ObservableRecipient
{
    public IThemeService ThemeService;
    public ThemeSettingViewModel(IThemeService themeService)
    {
        ThemeService = themeService;
    }

    [RelayCommand]
    private void OnBackdropChanged(object sender)
    {
        ThemeService.OnBackdropComboBoxSelectionChanged(sender);
    }

    [RelayCommand]
    private void OnThemeChanged(object sender)
    {
        ThemeService.OnThemeComboBoxSelectionChanged(sender);
    }

    [RelayCommand]
    private async void OpenWindowsColorSettings()
    {
        _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
    }
}
