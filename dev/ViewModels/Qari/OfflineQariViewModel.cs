using System.Collections.ObjectModel;

using Newtonsoft.Json;

namespace AlAnvar.ViewModels;
public partial class OfflineQariViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<QuranAudio> quranAudios;

    [ObservableProperty]
    public AdvancedCollectionView quranAudiosACV;

    [ObservableProperty]
    public string statusText;

    [ObservableProperty]
    public object listViewSelectedItem;

    public IList<object> listViewSelectedItems;

    public OfflineQariViewModel()
    {
        GetLocalAudios();
    }

    [RelayCommand]
    private void OnListViewItemChanged(object sender)
    {
        var listview = sender as ListView;
        listViewSelectedItems = listview?.SelectedItems;
        IsActive = listViewSelectedItems != null && listViewSelectedItems.Count > 0;
    }

    [RelayCommand]
    private void OnRemoveQari()
    {
        IsActive = false;
        DeleteQari();
    }

    [RelayCommand]
    private void OnRefresh()
    {
        GetLocalAudios();
    }

    [RelayCommand]
    private void DeleteQari()
    {
        if (listViewSelectedItems != null && listViewSelectedItems.Count > 0)
        {
            foreach (var item in listViewSelectedItems)
            {
                var audioId = (item as QuranAudio).DirName;
                var dirPath = Path.Combine(Settings.AudiosPath, audioId);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
            }
            GetLocalAudios();
        }
    }

    private void GetLocalAudios()
    {
        if (Directory.Exists(Settings.AudiosPath))
        {
            QuranAudios = new();
            var localFiles = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
            if (localFiles.Any())
            {
                foreach (var file in localFiles)
                {
                    try
                    {
                        var item = JsonConvert.DeserializeObject<QuranAudio>(File.ReadAllText(file));
                        QuranAudios.Add(item);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }
                }

                QuranAudiosACV = new AdvancedCollectionView(QuranAudios, true);
            }
        }
    }
}
