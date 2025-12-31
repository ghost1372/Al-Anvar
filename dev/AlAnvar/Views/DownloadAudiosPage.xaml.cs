using AlAnvar.Models;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class DownloadAudiosPage : Page
{
    public AudiosViewModel ViewModel { get; }
    public static DownloadAudiosPage Instance { get; private set; }
    public DownloadAudiosPage()
    {
        ViewModel = App.GetService<AudiosViewModel>();
        InitializeComponent();
        Instance = this;

        AudiosTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, ViewModel.Filter));

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.GetAvailableAudios();
    }

    private void OnDownloadAudio(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is AudioItem audioItem)
        {
            var window = new DownloadAudioWindow();
            WindowHelper.TrackWindow(window);
            window.AudioItem = audioItem;
            window.Activate();
        }
    }

    public TableView GetTableView()
    {
        return AudiosTableView;
    }
}
