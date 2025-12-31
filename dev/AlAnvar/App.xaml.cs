namespace AlAnvar;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public static Window MainWindow = Window.Current;
    public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
    public IServiceProvider Services { get; }
    public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
    public IThemeService ThemeService => GetService<IThemeService>();
    public static T GetService<T>() where T : class
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

        if (!Directory.Exists(Settings.TranslationsPath))
        {
            Directory.CreateDirectory(Settings.TranslationsPath);
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
        services.AddSingleton<TranslationsViewModel>();
        services.AddSingleton<AudiosViewModel>();

        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();
        services.AddTransient<TranslationSettingViewModel>();
        services.AddTransient<DatabaseSettingViewModel>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeLandingViewModel>();
        services.AddTransient<QuranViewModel>();
        services.AddTransient<AudioSettingViewModel>();
        services.AddTransient<QuranTabViewItemViewModel>();
        services.AddTransient<FavoriteTabViewItemViewModel>();
        services.AddTransient<NoteViewModel>();
        services.AddTransient<TafsirTabViewItemViewModel>();
        services.AddTransient<TafsirViewModel>();
        services.AddTransient<SearchViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;

        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");
        MainWindow.AppWindow.SetTaskbarIcon("Assets/AppIcon.ico");

        if (Settings.UIFont == null)
        {
            FontHelper.SetUIFontFamily(Constants.DefaultUIFont);
            Settings.UIFont = Constants.DefaultUIFont;
        }
        else
        {
            FontHelper.SetUIFontFamily(Settings.UIFont);
        }
        if (Settings.QuranFont == null)
        {
            FontHelper.SetQuranFontFamily(Constants.DefaultQuranFont);
            Settings.QuranFont = Constants.DefaultQuranFont;
        }
        else
        {
            FontHelper.SetQuranFontFamily(Settings.QuranFont);
        }
        if (Settings.TranslationFont == null)
        {
            FontHelper.SetTranslationFontFamily(Constants.DefaultTranslationFont);
            Settings.TranslationFont = Constants.DefaultTranslationFont;
        }
        else
        {
            FontHelper.SetTranslationFontFamily(Settings.TranslationFont);
        }

        ThemeService.Initialize(MainWindow);

        var manager = new WindowManager(MainWindow);
        manager.Height = 660;

        MainWindow.Activate();

        InitializeApp();

        ThemeService.ThemeChanged -= OnThemeChanged;
        ThemeService.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object sender, ElementTheme e)
    {
        if (!ProxyService.Instance.IsUserDefinedQuranColor)
        {
            ProxyService.Instance.QuranColor = Constants.DefaultTextBrush;
        }

        if (!ProxyService.Instance.IsUserDefinedTranslationColor)
        {
            ProxyService.Instance.TranslationColor = Constants.DefaultTextBrush;
        }

        if (!ProxyService.Instance.IsUserDefinedQuranNumberColor)
        {
            ProxyService.Instance.QuranNumberColor = Constants.DefaultTextBrush;
        }
    }

    private void InitializeApp()
    {
        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }
}

