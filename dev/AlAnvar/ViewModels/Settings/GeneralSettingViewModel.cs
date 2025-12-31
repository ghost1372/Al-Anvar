using System.Collections.ObjectModel;
using AlAnvar.Models;
using Microsoft.Windows.AppLifecycle;

namespace AlAnvar.ViewModels;

public partial class GeneralSettingViewModel : ObservableObject
{
    public ObservableCollection<AppLanguageItem> AppLanguages => AppLanguageHelper.SupportedLanguages;

    [ObservableProperty]
    public partial int SelectedAppLanguageIndex { get; set; }
    partial void OnSelectedAppLanguageIndexChanged(int value)
    {
        if (AppLanguageHelper.TryChange(value))
        {
            ShowRestartForLanguage = true;
        }
    }

    [ObservableProperty]
    public partial bool ShowRestartForLanguage { get; set; }

    [ObservableProperty]
    public partial bool ShowRestartForFont { get; set; }

    public GeneralSettingViewModel()
    {
        SelectedAppLanguageIndex = AppLanguageHelper.SupportedLanguages.IndexOf(AppLanguageHelper.PreferredLanguage);
    }

    [RelayCommand]
    public void OnRestart()
    {
        AppInstance.Restart(null);
    }

    [RelayCommand]
    public void OnCancelRestart()
    {
        ShowRestartForLanguage = false;
        ShowRestartForFont = false;
    }
}
