namespace AlAnvar.ViewModels;

public partial class MainViewModel
{
    public IJsonNavigationViewService JsonNavigationViewService;
    public IThemeService ThemeService;
    public MainViewModel(IJsonNavigationViewService jsonNavigationViewService, IThemeService themeService)
    {
        ThemeService = themeService;
        JsonNavigationViewService = jsonNavigationViewService;
        themeService.Initialize(App.currentWindow);
        themeService.ConfigBackdrop();
        themeService.ConfigElementTheme();
        themeService.ConfigBackdropFallBackColorForWindow10(Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush);
    }
}
