namespace AlAnvar.Views;

public sealed partial class QariPage : Page
{
    public static QariPage Instance { get; set; }
    public QariPage()
    {
        this.InitializeComponent();
        Instance = this;
        Loaded += QariPage_Loaded;
    }

    private void QariPage_Loaded(object sender, RoutedEventArgs e)
    {
        segmented.SelectedIndex = 0;
    }

    public Frame GetFrame()
    {
        return qariFrame;
    }

    private void OnSegmentedChanged(object sender, SelectionChangedEventArgs e)
    {
        var segmented = sender as Segmented;
        if (qariFrame != null)
        {
            if (segmented.SelectedIndex == 0)
            {
                qariFrame.Navigate(typeof(DownloadQariPage));
            }
            else
            {
                qariFrame.Navigate(typeof(OfflineQariPage));
            }
        }
    }
}
