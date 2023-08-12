using Microsoft.UI;

namespace AlAnvar;

public partial class App : Application
{
    public static Window currentWindow = Window.Current;
    public string AlAnvarVersion { get; set; }
    public IServiceProvider Services { get; }
    public ResourceHelper ResourceHelper { get; set; }
    public new static App Current => (App) Application.Current;

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public App()
    {
        Services = ConfigureServices();
        this.InitializeComponent();
        AlAnvarVersion = VersionHelper.GetVersion();
        if (!Directory.Exists(Settings.AudiosPath))
        {
            Directory.CreateDirectory(Settings.AudiosPath);
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IJsonNavigationViewService>(factory =>
        {
            var json = new JsonNavigationViewService();
            json.ConfigDefaultPage(typeof(HomeLandingPage));
            json.ConfigSettingsPage(typeof(SettingsPage));
            return json;
        });
        services.AddSingleton<IThemeService, ThemeService>();

        services.AddTransient<HomeLandingPage>();
        services.AddTransient<MainPage>();
        services.AddTransient<QariPage>();
        services.AddTransient<DownloadQariPage>();
        services.AddTransient<OfflineQariPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<QuranPage>();

        services.AddTransient<BreadCrumbBarViewModel>();
        services.AddTransient<HomeLandingViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DownloadQariViewModel>();
        services.AddTransient<OfflineQariViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QuranViewModel>();

        //Settings
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<FontSettingViewModel>();
        services.AddTransient<QariSettingViewModel>();
        services.AddTransient<ThemeSettingViewModel>();
        services.AddTransient<TranslationSettingViewModel>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AboutViewModel>();

        services.AddTransient<AppUpdateSettingPage>();
        services.AddTransient<FontSettingPage>();
        services.AddTransient<QariSettingPage>();
        services.AddTransient<ThemeSettingPage>();
        services.AddTransient<TranslationSettingPage>();
        services.AddTransient<GeneralSettingPage>();
        services.AddTransient<AboutSettingPage>();


        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        currentWindow = new Window();

        currentWindow.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        currentWindow.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;

        if (currentWindow.Content is not Frame rootFrame)
        {
            currentWindow.Content = rootFrame = new Frame();
        }

        rootFrame.Navigate(typeof(MainPage));

        currentWindow.Title = currentWindow.AppWindow.Title = $"TvTime v{AlAnvarVersion}";
        currentWindow.AppWindow.SetIcon("Assets/Fluent/icon.ico");

        currentWindow.Activate();
    }
}
