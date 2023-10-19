namespace AlAnvar.Views;

public sealed partial class ChangeFontContentDialog : ContentDialog
{
    public FontType FontType { get; set; }
    
    public ChangeFontContentDialog()
    {
        this.InitializeComponent();
        XamlRoot = App.currentWindow.Content.XamlRoot;
        Loaded += ChangeFontContentDialog_Loaded;
    }

    private void ChangeFontContentDialog_Loaded(object sender, RoutedEventArgs e)
    {
        RequestedTheme = MainPage.Instance.ViewModel.ThemeService.GetCurrentTheme();
        LoadComboBoxItems();
        switch (FontType)
        {
            case FontType.Aya:
                txtResult.Text = "بسم الله الرحمن الرحیم";
                nbSize.Value = txtResult.FontSize = Settings.AyatFontSize;
                cmbFont.SelectedItem = cmbFont.Items.Where(x => ((ComboBoxItem) x).Tag.ToString().Equals(Settings.AyatFontFamilyName)).FirstOrDefault();
                break;
            case FontType.AyaNumber:
                txtResult.Text = "(1:7)";
                nbSize.Value = txtResult.FontSize = Settings.AyatNumberFontSize;
                cmbFont.SelectedItem = cmbFont.Items.Where(x => ((ComboBoxItem) x).Tag.ToString().Equals(Settings.AyatNumberFontFamilyName)).FirstOrDefault();
                break;
            case FontType.Translation:
                txtResult.Text = "بنام خداوند بخشنده مهربان";
                nbSize.Value = txtResult.FontSize = Settings.TranslationFontSize;
                cmbFont.SelectedItem = cmbFont.Items.Where(x => ((ComboBoxItem) x).Tag.ToString().Equals(Settings.TranslationFontFamilyName)).FirstOrDefault();
                break;
        }
    }

    public void LoadComboBoxItems()
    {
        cmbFont.Items.Clear();
        if (chkSystemFont.IsChecked.Value)
        {
            var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            foreach (var item in systemFonts)
            {
                cmbFont.Items.Add(new ComboBoxItem { Content = item, Tag = item });
            }
        }
        else
        {
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = IRANSANS_FONT_PERSIAN,
                Tag = IRANSANS_FONT_ASSET
            });
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = VAZIRMATN_FONT_PERSIAN,
                Tag = VAZIRMATN_FONT_ASSET
            });
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = IRANYEKAN_FONT_PERSIAN,
                Tag = IRANYEKAN_FONT_ASSET
            });
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = NABI_FONT_PERSIAN,
                Tag = NABI_FONT_ASSET
            });
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = NEIRIZI_FONT_PERSIAN,
                Tag = NEIRIZI_FONT_ASSET
            });
            cmbFont.Items.Add(new ComboBoxItem
            {
                Content = QURANTAHA_FONT_PERSIAN,
                Tag = QURANTAHA_FONT_ASSET
            });
        }
    }

    private void cmbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = cmbFont.SelectedItem as ComboBoxItem;
        if (item != null)
        {
            switch (FontType)
            {
                case FontType.Aya:
                    Settings.AyatFontFamilyName = item.Tag.ToString();
                    break;
                case FontType.AyaNumber:
                    Settings.AyatNumberFontFamilyName = item.Tag.ToString();
                    break;
                case FontType.Translation:
                    Settings.TranslationFontFamilyName = item.Tag.ToString();
                    break;
            }
            txtResult.FontFamily = new FontFamily(item.Tag.ToString());
        }
    }

    private void chkSystemFont_Checked(object sender, RoutedEventArgs e)
    {
        LoadComboBoxItems();
    }

    private void nbSize_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        txtResult.FontSize = args.NewValue;

        switch (FontType)
        {
            case FontType.Aya:
                Settings.AyatFontSize = args.NewValue;
                break;
            case FontType.AyaNumber:
                Settings.AyatNumberFontSize = args.NewValue;
                break;
            case FontType.Translation:
                Settings.TranslationFontSize = args.NewValue;
                break;
        }
    }
}
public enum FontType
{
    Aya,
    AyaNumber,
    Translation
}
