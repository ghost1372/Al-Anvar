namespace AlAnvar.UI.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        ThemeHelper.SetThemeRadioButtonChecked(ThemePanel);
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadFontsInCombobox();
        GetDefaultColors();
        GetDefaultFonts();
        LoadTranslationsInCombobox();
    }

    private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        ThemeHelper.OnThemeRadioButtonChecked(sender);
    }

    private async void WindowsColorSettings_Click(object sender, RoutedEventArgs e)
    {
        _= await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
    }

    private void LoadFontsInCombobox()
    {
        cmbFonts.Items.Clear();
        if (chkSystemFonts.IsChecked.Value)
        {
            var systemFonts = System.Drawing.FontFamily.Families;
            foreach (var item in systemFonts)
            {
                cmbFonts.Items.Add(item.Name);
            }
        }
        else
        {
            //Todo:
            List<string> fonts = new List<string>();

        }
    }

    private void chkSystemFonts_Checked(object sender, RoutedEventArgs e)
    {
        LoadFontsInCombobox();
    }

    private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var selectedColor = new SolidColorBrush(args.NewColor);
        if (rbAyat.IsChecked.Value)
        {
            txtAyat.Foreground = selectedColor;
            Settings.AyatForeground = args.NewColor.ToString();
        }
        else if (rbAyatNumber.IsChecked.Value)
        {
            txtAyatNumber.Foreground = selectedColor;
            Settings.AyatNumberForeground = args.NewColor.ToString();
        }
        else if (rbTranslation.IsChecked.Value)
        {
            txtTranslation.Foreground = selectedColor;
            Settings.TranslationForeground = args.NewColor.ToString();
        }
    }
    private void GetDefaultColors()
    {
        if (Settings.AyatForeground != null)
        {
            txtAyat.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
        }

        if (Settings.AyatNumberForeground != null)
        {
            txtAyatNumber.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
        }

        if (Settings.TranslationForeground != null)
        {
            txtTranslation.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
        }
    }

    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Constants.TranslationsPath))
        {
            var files = Directory.GetFiles(Constants.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(file));
                    if (trans != null)
                    {
                        cmbTranslators.Items.Add(trans);
                    }
                }
            }

            cmbTranslators.SelectedItem = Settings.QuranTranslation;
        }
    }
    private void GetDefaultFonts()
    {
        if (Settings.AyatFontFamilyName != null)
        {
            txtAyat2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatFontFamilyName);
        }

        if (Settings.AyatNumberFontFamilyName != null)
        {
            txtAyatNumber2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatNumberFontFamilyName);
        }

        if (Settings.TranslationFontFamilyName != null)
        {
            txtTranslation2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.TranslationFontFamilyName);
        }

        txtAyat2.FontSize = Settings.AyatFontSize;
        txtAyatNumber2.FontSize = Settings.AyatNumberFontSize;
        txtTranslation2.FontSize = Settings.TranslationFontSize;
    }
    private void btnResetColors_Click(object sender, RoutedEventArgs e)
    {
        Settings.AyatForeground = null;
        Settings.AyatNumberForeground = null;
        Settings.TranslationForeground = null;
        GetDefaultColors();
    }

    private void cmbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedFont = cmbFonts.SelectedItem;
        var fontFamily = new Microsoft.UI.Xaml.Media.FontFamily(selectedFont as string);
        switch (rbFont.SelectedIndex)
        {
            case 0:
                txtAyat2.FontFamily = fontFamily;
                Settings.AyatFontFamilyName = selectedFont as string;
                break;
            case 1:
                txtTranslation2.FontFamily = fontFamily;
                Settings.TranslationFontFamilyName = selectedFont as string;
                break;
            case 2:
                txtAyatNumber2.FontFamily = fontFamily;
                Settings.AyatNumberFontFamilyName = selectedFont as string;
                break;
        }
    }

    private void nbFontSize_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (rbFont == null)
        {
            return;
        }
        switch (rbFont.SelectedIndex)
        {
            case 0:
                txtAyat2.FontSize = args.NewValue;
                Settings.AyatFontSize = args.NewValue;
                break;
            case 1:
                txtTranslation2.FontSize = args.NewValue;
                Settings.TranslationFontSize = args.NewValue;
                break;
            case 2:
                txtAyatNumber2.FontSize = args.NewValue;
                Settings.AyatNumberFontSize = args.NewValue;
                break;
        }
    }

    private void btnResetFont_Click(object sender, RoutedEventArgs e)
    {
        Settings.AyatFontFamilyName = null;
        Settings.AyatNumberFontFamilyName = null;
        Settings.TranslationFontFamilyName = null;
        Settings.AyatFontSize = 24;
        Settings.AyatNumberFontSize = 24;
        Settings.TranslationFontSize = 24;
        GetDefaultFonts();
    }

    private void rbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (rbColor.SelectedIndex)
        {
            case 0:
                if (Settings.AyatForeground != null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.AyatForeground);
                }
                break;
            case 1:
                if (Settings.TranslationForeground != null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.TranslationForeground);
                }
                break;
            case 2:
                if (Settings.AyatNumberForeground != null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.AyatNumberForeground);
                }
                break;
        }
    }

    private void rbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (rbFont.SelectedIndex)
        {
            case 0:
                double ayatFontSize = Settings.AyatFontSize;
                txtAyat2.FontSize = ayatFontSize;
                nbFontSize.Value = ayatFontSize;
                if (Settings.AyatFontFamilyName != null)
                {
                    txtAyat2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatFontFamilyName);
                    cmbFonts.SelectedItem = Settings.AyatFontFamilyName;
                }
                break;
            case 1:
                double translationFontSize = Settings.TranslationFontSize;
                txtTranslation2.FontSize = translationFontSize;
                nbFontSize.Value = translationFontSize;
                if (Settings.TranslationFontFamilyName != null)
                {
                    txtTranslation2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.TranslationFontFamilyName);
                    cmbFonts.SelectedItem = Settings.TranslationFontFamilyName;
                }
                break;
            case 2:
                double ayatNumberFontSize = Settings.AyatNumberFontSize;
                txtAyatNumber2.FontSize = ayatNumberFontSize;
                nbFontSize.Value = ayatNumberFontSize;
                if (Settings.AyatNumberFontFamilyName != null)
                {
                    txtAyatNumber2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatNumberFontFamilyName);
                    cmbFonts.SelectedItem = Settings.AyatNumberFontFamilyName;
                }
                break;
        }
    }

    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;
    }

    private void btnMoreTranslation_Click(object sender, RoutedEventArgs e)
    {
        ShellPage.Instance.GetFrame().Navigate(typeof(TranslationPage), null, new EntranceNavigationTransitionInfo());
    }
}
