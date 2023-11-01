using System.Collections.ObjectModel;

using Newtonsoft.Json;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace AlAnvar.ViewModels;

public partial class QariSettingViewModel : ObservableRecipient
{
    [ObservableProperty]
    public ObservableCollection<QuranAudio> qarisCollection = new();

    [ObservableProperty]
    public QuranAudio currentQari;

    [ObservableProperty]
    public int qariIndex;

    [ObservableProperty]
    public string audioFolderPath = Settings.AudiosPath;

    private IJsonNavigationViewService jsonNavigationViewService;

    public QariSettingViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        this.jsonNavigationViewService = jsonNavigationViewService;
        LoadQaris();
    }

    [RelayCommand]
    private void GoToQariPage()
    {
        jsonNavigationViewService.NavigateTo(typeof(QariPage));
    }

    private void LoadQaris()
    {
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            QarisCollection?.Clear();
            if (Directory.Exists(Settings.AudiosPath))
            {
                var files = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var audios = JsonConvert.DeserializeObject<QuranAudio>(File.ReadAllText(file));
                            if (audios is not null)
                            {
                                QarisCollection.Add(audios);
                            }
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                }

                if (QarisCollection.Any())
                {
                    CurrentQari = QarisCollection.Where(audio => ((QuranAudio) audio).DirName == Settings.QuranAudio?.DirName).FirstOrDefault();
                    QariIndex = QarisCollection.IndexOf(CurrentQari);
                }
            }
        });
    }

    [RelayCommand]
    private void OnQariItemChanged(object sender)
    {
        var cmbQari = sender as ComboBox;
        if (cmbQari.SelectedItem != null)
        {
            Settings.QuranAudio = cmbQari.SelectedItem as QuranAudio;
        }
    }

    [RelayCommand]
    private async void OnLaunchAudioPath()
    {
        await Launcher.LaunchUriAsync(new Uri(Settings.AudiosPath));
    }

    [RelayCommand]
    private async void OnChooseAudioPath()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WindowHelper.GetWindowHandleForCurrentWindow(App.currentWindow));

        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder is not null)
        {
            Settings.AudiosPath = folder.Path;
            AudioFolderPath = folder.Path;
            LoadQaris();
        }
    }
}
