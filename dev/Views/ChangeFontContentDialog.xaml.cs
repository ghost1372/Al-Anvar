namespace AlAnvar.Views;

public sealed partial class ChangeFontContentDialog : ContentDialog
{
    public class FontType2
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public FontType2(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }
    }

    public FontType FontType { get; set; }

    List<FontType2> BuiltInFonts { get; set; } = new List<FontType2>
    {
        new FontType2("ایران سنس", "ms-appx:///Assets/Fonts/IRANSansX-Regular.ttf#IRANSansX"),
        new FontType2("وزیر متن", "ms-appx:///Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn"),
        new FontType2("ایران یکان", "ms-appx:///Assets/Fonts/IRANYekanRegular.ttf#IRANYekan"),
        new FontType2("نبی", "ms-appx:///Assets/Fonts/Nabi.ttf#Nabi"),
        new FontType2("نیریزی", "ms-appx:///Assets/Fonts/Neirizi.ttf#Neirizi"),
        new FontType2("قرآن طه", "ms-appx:///Assets/Fonts/QuranTaha.ttf#QuranTaha"),
        new FontType2("الکلامی", "ms-appx:///Assets/Fonts/Alkalami-Regular.ttf#Alkalami"),
        new FontType2("هرمتان", "ms-appx:///Assets/Fonts/Harmattan-Regular.ttf#Harmattan"),
        new FontType2("روودو", "ms-appx:///Assets/Fonts/Ruwudu-Regular.ttf#Ruwudu"),
        new FontType2("عثمان طه نسخ", "ms-appx:///Assets/Fonts/UthmanTN_v2-0.ttf#KFGQPC Uthman Taha Naskh"),
        new FontType2("کوفی", "ms-appx:///Assets/Fonts/KFGQPC KufiStyV13.ttf#KFGQPC Kufi Stylistic"),
        new FontType2("حفس", "ms-appx:///Assets/Fonts/UthmanicHafs_v2-1.ttf#KFGQPC HAFS Uthmanic Script")
    };
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
            foreach (var item in BuiltInFonts)
            {
                cmbFont.Items.Add(new ComboBoxItem
                {
                    Content = item.Name,
                    Tag = item.Path
                });
            }
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
