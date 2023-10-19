namespace AlAnvar.Views;
public sealed partial class QuranItemUC : UserControl
{
    public QuranViewModel ViewModel
    {
        get { return (QuranViewModel) GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(QuranViewModel), typeof(QuranItemUC), new PropertyMetadata(null));

    public string AyahText
    {
        get { return (string) GetValue(AyahTextProperty); }
        set { SetValue(AyahTextProperty, value); }
    }

    public static readonly DependencyProperty AyahTextProperty =
        DependencyProperty.Register("AyahText", typeof(string), typeof(QuranItemUC), new PropertyMetadata(default(string)));

    public string TranslationText
    {
        get { return (string) GetValue(TranslationTextProperty); }
        set { SetValue(TranslationTextProperty, value); }
    }

    public static readonly DependencyProperty TranslationTextProperty =
        DependencyProperty.Register("TranslationText", typeof(string), typeof(QuranItemUC), new PropertyMetadata(default(string)));

    public string AyaDetail
    {
        get { return (string) GetValue(AyaDetailProperty); }
        set { SetValue(AyaDetailProperty, value); }
    }

    public static readonly DependencyProperty AyaDetailProperty =
        DependencyProperty.Register("AyaDetail", typeof(string), typeof(QuranItemUC), new PropertyMetadata(default(string)));

    public static QuranItemUC Instance { get; set; }
    public QuranItemUC()
    {
        this.InitializeComponent();
        Instance = this;

        if (QuranTabViewItem.Instance != null)
        {
            ViewModel = QuranTabViewItem.Instance.viewModel;
        }
    }

    private void TextBlock_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }
        var textblock = sender as TextBlock;
        if (string.IsNullOrEmpty(textblock.Text))
        {
            textblock.Visibility = Visibility.Collapsed;
        }
        switch (textblock?.Tag?.ToString())
        {
            case "Ayat":
                if (Settings.AyatForeground != null)
                {
                    textblock.Foreground = ViewModel.AyatForeground;
                }
                break;
            case "AyatNumber":
                if (Settings.AyatNumberForeground != null)
                {
                    textblock.Foreground = ViewModel.AyatNumberForeground;
                }
                break;
            case "Translation":
                if (Settings.TranslationForeground != null)
                {
                    textblock.Foreground = ViewModel.TranslationForeground;
                }
                break;
        }
    }
}
