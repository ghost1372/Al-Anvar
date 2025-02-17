namespace AlAnvar;

public partial class App : Application
{
    public static Window MainWindow = Window.Current;
    public new static App Current => (App) Application.Current;
    public IServiceProvider Services { get; }
    public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
    public IThemeService ThemeService => GetService<IThemeService>();
    public ResourceHelper ResourceHelper { get; set; }

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
        if (!Directory.Exists(Settings.AudiosPath))
        {
            Directory.CreateDirectory(Settings.AudiosPath);
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();

        services.AddTransient<DownloadQariViewModel>();
        services.AddTransient<OfflineQariViewModel>();
        services.AddTransient<DownloadTranslationViewModel>();
        services.AddTransient<OfflineTranslationViewModel>();
        services.AddTransient<QuranViewModel>();

        //Settings
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<FontSettingViewModel>();
        services.AddTransient<QariSettingViewModel>();
        services.AddTransient<TranslationSettingViewModel>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AboutViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        if (this.ThemeService != null)
        {
            this.ThemeService.AutoInitialize(MainWindow)
                .ConfigureTintColor();
        }

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/icon.ico");

        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }
        UnhandledException += App_UnhandledException;

        MainWindow.Activate();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Logger?.Error(e.Exception, "UnhandledException");
    }
}
