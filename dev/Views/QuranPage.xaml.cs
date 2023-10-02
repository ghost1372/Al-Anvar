namespace AlAnvar.Views;

public sealed partial class QuranPage : Page
{
    public static QuranPage Instance { get; set; }
    public QuranViewModel ViewModel { get; }
    public QuranPage()
    {
        ViewModel = App.GetService<QuranViewModel>();
        this.InitializeComponent();
        Instance = this;
    }
}
