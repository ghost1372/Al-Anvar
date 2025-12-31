namespace AlAnvar.Views;

public sealed partial class SettingsPage : Page
{
    public NavigationParameterExtension GeneralParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(GeneralSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_General_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension DatabaseParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(DatabaseSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_Database_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension TranslationsParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(TranslationSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_Translations_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension AudiosParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(AudioSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_Audios_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension ThemeParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(ThemeSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_Theme_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension UpdateParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(AppUpdateSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_Update_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension AboutParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(AboutUsSettingPage),
        BreadCrumbHeader = Strings.SettingsPage_About_Header.GetLocalizedResource()
    };
    
    public SettingsPage()
    {
        this.InitializeComponent();

        var glyph = GeneralHelper.GetGlyph(Strings.SettingsPage_ActionIcon.GetLocalizedResource());
        GeneralCard.ActionIcon = new FontIcon { Glyph = glyph };
        DatabaseCard.ActionIcon = new FontIcon { Glyph = glyph };
        TranslationCard.ActionIcon = new FontIcon { Glyph = glyph };
        AudioCard.ActionIcon = new FontIcon { Glyph = glyph };
        ThemeCard.ActionIcon = new FontIcon { Glyph = glyph };
        UpdateCard.ActionIcon = new FontIcon { Glyph = glyph };
        AboutCard.ActionIcon = new FontIcon { Glyph = glyph };
    }
}

