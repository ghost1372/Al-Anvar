using AlAnvar.Models;
using Windows.System;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class LocalAudiosPage : Page
{
    public AudiosViewModel ViewModel { get; }
    public static LocalAudiosPage Instance { get; private set; }
    public LocalAudiosPage()
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
        await ViewModel.GetAvailableLocalAudios();
    }

    private async void AudiosTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AudiosTableView.SelectedItems.Count > 0)
        {
            BtnDeleteSelection.IsEnabled = true;
        }
        else
        {
            BtnDeleteSelection.IsEnabled = false;
        }

        if (AudiosTableView.SelectedItems.Count == 1)
        {
            BtnOpenFolder.IsEnabled = true;
        }
        else
        {
            BtnOpenFolder.IsEnabled = false;
        }
    }

    private async void OnDeleteSelection(object sender, RoutedEventArgs e)
    {
        var items = AudiosTableView.SelectedItems.OfType<AudioItem>().ToList();
        if (items != null && items.Count > 0)
        {
            var result = await MessageBox.ShowWarningAsync(Strings.LocalAudiosPage_MessageBoxConfirmDelete.GetLocalizedResource(), MessageBoxButtons.YesNo);
            if (result == MessageBoxResult.YES)
            {
                await ViewModel.DeleteSelections(items);
                await ViewModel.GetAvailableLocalAudios();
            }
        }
    }
    private async void OnOpenFolder(object sender, RoutedEventArgs e)
    {
        if (AudiosTableView.SelectedItem is AudioItem item)
        {
            var dir = Path.Combine(Settings.AudiosPath, item.DirName);
            if (Directory.Exists(dir))
            {
                await Launcher.LaunchFolderPathAsync(dir);
            }
            else
            {
                await MessageBox.ShowErrorAsync(Strings.LocalAudiosPage_MessageBoxFileNotFound.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
            }
        }
    }

    public TableView GetTableView()
    {
        return AudiosTableView;
    }
}
