namespace AlAnvar.ViewModels;

public partial class FontSettingViewModel : ObservableObject
{
    #region Change Color
    [ObservableProperty]
    public Brush txtAyaForeground;

    [ObservableProperty]
    public Brush txtAyaNumberForeground;

    [ObservableProperty]
    public Brush txtTranslationForeground;

    #endregion

    #region Change Font
    [ObservableProperty]
    public FontFamily txtAyaFontFamily = new FontFamily(Settings.AyatFontFamilyName);

    [ObservableProperty]
    public FontFamily txtAyaNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);

    [ObservableProperty]
    public FontFamily txtTranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

    #endregion

    public FontSettingViewModel()
    {
        GetDefaultColors();
        GetDefaultFonts();
    }

    #region Change Color
    [RelayCommand]
    private async Task OnChooseColor(string settingType)
    {
        var scrollViewer = new ScrollViewer { Margin = new Thickness(10) };
        var colorPicker = new ColorPicker
        {
            ColorSpectrumShape = ColorSpectrumShape.Ring,
            IsMoreButtonVisible = false,
            IsColorSliderVisible = true,
            IsColorChannelTextInputVisible = false,
            IsHexInputVisible = true,
            IsAlphaEnabled = false,
            IsAlphaSliderVisible = false,
            IsAlphaTextInputVisible = false,
            Margin = new Thickness(10)
        };

        colorPicker.ColorChanged += (s, e) =>
        {
            try
            {
                var selectedColor = new SolidColorBrush(e.NewColor);
                switch (settingType)
                {
                    case "Aya":
                        TxtAyaForeground = selectedColor;
                        Settings.AyatForeground = e.NewColor.ToString();
                        break;
                    case "AyaNumber":
                        TxtAyaNumberForeground = selectedColor;
                        Settings.AyatNumberForeground = e.NewColor.ToString();
                        break;
                    case "Translation":
                        TxtTranslationForeground = selectedColor;
                        Settings.TranslationForeground = e.NewColor.ToString();
                        break;
                }

                FontSettingPage.Instance.RefreshTextBlockForeground();
            }
            catch (Exception)
            {
            }
        };

        scrollViewer.Content = colorPicker;
        ContentDialog contentDialog = new ContentDialog();
        contentDialog.XamlRoot = App.currentWindow.Content.XamlRoot;
        contentDialog.Loaded += (s, e) =>
        {
            contentDialog.Content = scrollViewer;
        };
        contentDialog.Title = "انتخاب رنگ";
        contentDialog.PrimaryButtonText = "تایید";
        contentDialog.SecondaryButtonText = "انصراف";
        contentDialog.PrimaryButtonStyle = (Style) Application.Current.Resources["AccentButtonStyle"];
        contentDialog.FlowDirection = FlowDirection.RightToLeft;
        await contentDialog.ShowAsyncQueue();
    }

    [RelayCommand]
    private void OnResetColors()
    {
        Settings.AyatForeground = null;
        Settings.AyatNumberForeground = null;
        Settings.TranslationForeground = null;
        GetDefaultColors();
        FontSettingPage.Instance.RefreshTextBlockForeground2();
    }

    private void GetDefaultColors()
    {
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            if (Settings.AyatForeground is not null)
            {
                TxtAyaForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
            }

            if (Settings.AyatNumberForeground is not null)
            {
                TxtAyaNumberForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
            }

            if (Settings.TranslationForeground is not null)
            {
                TxtTranslationForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
            }
        });
    }

    #endregion

    #region Change Font

    [RelayCommand]
    private async Task OnChooseFont(string settingType)
    {
        var changeFont = new ChangeFontContentDialog();
        switch (settingType)
        {
            case "Aya":
                changeFont.FontType = FontType.Aya;
                break;
            case "AyaNumber":
                changeFont.FontType = FontType.AyaNumber;
                break;
            case "Translation":
                changeFont.FontType = FontType.Translation;
                break;
        }
        await changeFont.ShowAsync();
        GetDefaultFonts();

    }

    [RelayCommand]
    private void OnResetFonts()
    {
        Settings.AyatFontFamilyName = AYAT_DEFAULT_FONT_NAME;
        Settings.AyatNumberFontFamilyName = AYAT_NUMBER_DEFAULT_FONT_NAME;
        Settings.TranslationFontFamilyName = TRANSLATION_DEFAULT_FONT_NAME;
        Settings.AyatFontSize = AYAT_DEFAULT_FONT_SIZE;
        Settings.AyatNumberFontSize = AYAT_NUMBER_DEFAULT_FONT_SIZE;
        Settings.TranslationFontSize = TRANSLATION_DEFAULT_FONT_SIZE;
        GetDefaultFonts();
    }

    private void GetDefaultFonts()
    {
        TxtAyaFontFamily = new FontFamily(Settings.AyatFontFamilyName);
        TxtAyaNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
        TxtTranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);
    }
    #endregion
}
