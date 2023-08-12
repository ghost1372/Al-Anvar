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
    private async void OnChooseColor(string settingType)
    {
        var scrollViewer = new ScrollViewer { Margin = new Thickness(10) };
        var colorPicker = new ColorPicker
        {
            ColorSpectrumShape = ColorSpectrumShape.Ring,
            IsMoreButtonVisible = false,
            IsColorSliderVisible = true,
            IsColorChannelTextInputVisible = false,
            IsHexInputVisible = false,
            IsAlphaEnabled = false,
            IsAlphaSliderVisible = false,
            IsAlphaTextInputVisible = false,
            Margin = new Thickness(10)
        };

        colorPicker.ColorChanged += (s, e) =>
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
    private FontFamily GetFontFamilyFromFontName(string fontname)
    {
        switch (fontname)
        {
            case "وزیرمتن رگولار":
                return new FontFamily(FONT_REGULAR);
            case "وزیرمتن مدیوم":
                return new FontFamily(FONT_MEDIUM);
            case "وزیرمتن بولد":
                return new FontFamily(FONT_BOLD);
        }
        return new FontFamily(FONT_REGULAR);
    }

    [RelayCommand]
    private async void OnChooseFont(string settingType)
    {
        var scrollViewer = new ScrollViewer { Margin = new Thickness(10) };
        var stackPanel = new StackPanel { Margin = new Thickness(10), Spacing = 10 };
        var textBlock = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center };
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Header = "قلم های موجود"
        };
        var checkBox = new CheckBox
        {
            Content = "استفاده از فونت های پیشفرض ویندوز",
            IsChecked = Settings.IsUsingSystemFonts
        };
        var numberBox = new NumberBox
        {
            Minimum = 6,
            Height = 34,
            Maximum = 48,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };
        void loadComboBoxItems()
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                comboBox.Items.Clear();
                if (checkBox.IsChecked.Value)
                {
                    var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
                    foreach (var item in systemFonts)
                    {
                        comboBox.Items.Add(item);
                    }
                }
                else
                {
                    //Todo:
                    comboBox.Items.Add("وزیرمتن رگولار");
                    comboBox.Items.Add("وزیرمتن مدیوم");
                    comboBox.Items.Add("وزیرمتن بولد");
                }
            });
        }
        loadComboBoxItems();
        switch (settingType)
        {
            case "Aya":
                textBlock.Text = "بسم الله الرحمن الرحیم";
                numberBox.Value = textBlock.FontSize = Settings.AyatFontSize;
                break;
            case "AyaNumber":
                textBlock.Text = "(1:7)";
                numberBox.Value = textBlock.FontSize = Settings.AyatNumberFontSize;
                break;
            case "Translation":
                textBlock.Text = "بنام خداوند بخشنده مهربان";
                numberBox.Value = textBlock.FontSize = Settings.TranslationFontSize;
                break;
        }

        comboBox.SelectionChanged += (s, e) =>
        {
            var selectedFont = comboBox.SelectedItem;
            if (selectedFont != null)
            {
                FontFamily fontFamily;
                if (Settings.IsUsingSystemFonts)
                {
                    fontFamily = new FontFamily(selectedFont as string);
                }
                else
                {
                    fontFamily = GetFontFamilyFromFontName(selectedFont as string);
                }
                switch (settingType)
                {
                    case "Aya":
                        TxtAyaFontFamily = fontFamily;
                        Settings.AyatFontFamilyName = selectedFont as string;
                        break;
                    case "AyaNumber":
                        TxtAyaNumberFontFamily = fontFamily;
                        Settings.AyatNumberFontFamilyName = selectedFont as string;
                        break;
                    case "Translation":
                        TxtTranslationFontFamily = fontFamily;
                        Settings.TranslationFontFamilyName = selectedFont as string;
                        break;
                }
                textBlock.FontFamily = fontFamily;
            }
        };

        checkBox.Checked += (s, e) =>
        {
            Settings.IsUsingSystemFonts = checkBox.IsChecked.Value;
            loadComboBoxItems();
        };
        checkBox.Unchecked += (s, e) =>
        {
            Settings.IsUsingSystemFonts = checkBox.IsChecked.Value;
            loadComboBoxItems();
        };

        numberBox.ValueChanged += (s, e) =>
        {
            textBlock.FontSize = e.NewValue;

            switch (settingType)
            {
                case "Aya":
                    Settings.AyatFontSize = e.NewValue;
                    break;
                case "AyaNumber":
                    Settings.AyatNumberFontSize = e.NewValue;
                    break;
                case "Translation":
                    Settings.TranslationFontSize = e.NewValue;
                    break;
            }
        };

        stackPanel.Children.Add(checkBox);
        stackPanel.Children.Add(comboBox);
        stackPanel.Children.Add(numberBox);
        stackPanel.Children.Add(textBlock);

        scrollViewer.Content = stackPanel;
        ContentDialog contentDialog = new ContentDialog();
        contentDialog.XamlRoot = App.currentWindow.Content.XamlRoot;
        contentDialog.Loaded += (s, e) =>
        {
            contentDialog.Content = scrollViewer;
        };
        contentDialog.Title = "انتخاب قلم";
        contentDialog.PrimaryButtonText = "تایید";
        contentDialog.FlowDirection = FlowDirection.RightToLeft;
        await contentDialog.ShowAsyncQueue();
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
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            TxtAyaFontFamily = new FontFamily(Settings.AyatFontFamilyName);
            TxtAyaNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
            TxtTranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);
        });
    }
    #endregion
}
