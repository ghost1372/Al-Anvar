using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

using Downloader;

namespace AlAnvar.ViewModels;
public partial class DownloadTranslationViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    private ObservableCollection<QuranTranslation> quranTranslations;

    [ObservableProperty]
    public AdvancedCollectionView quranTranslationsACV;

    [ObservableProperty]
    public bool isDownloadActive = true;

    [ObservableProperty]
    public bool isCancelActive;

    [ObservableProperty]
    public double progressValue;

    [ObservableProperty]
    public object listViewSelectedItem;

    private DownloadService downloadService;

    [RelayCommand]
    private async void OnPageLoaded()
    {
        IsActive = true;
        IsDownloadActive = false;
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();
            var data = await db.Translations.Where(x => x.IsActive == false).ToListAsync();
            QuranTranslations = new(data);
            dispatcherQueue.TryEnqueue(() =>
            {
                QuranTranslationsACV = new AdvancedCollectionView(QuranTranslations, true);
            });
        });
        IsActive = false;
        IsDownloadActive = true;
    }

    public void Search(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (QuranTranslationsACV != null)
        {
            QuranTranslationsACV.Filter = _ => true;
            QuranTranslationsACV.Filter = TranslationFilter;
        }
    }

    private bool TranslationFilter(object translation)
    {
        var query = translation as QuranTranslation;

        var name = query.Name ?? "";
        var language = query.Language ?? "";
        var translator = query.Translator ?? "";

        var txtSearch = MainPage.Instance.GetTxtSearch();
        return name.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)
            || language.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)
            || translator.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private void OnListViewItemChanged()
    {
        IsDownloadActive = ListViewSelectedItem != null;
    }

    [RelayCommand]
    private void OnDownloadTranslation()
    {
        IsDownloadActive = false;
        IsCancelActive = true;
        DownloadTranslationAsync();
    }

    [RelayCommand]
    private void OnCancelDownlaod()
    {
        IsDownloadActive = true;
        IsCancelActive = false;
        CancelDownload();
    }

    private void CancelDownload()
    {
        downloadService?.CancelAsync();
    }

    private async void DownloadTranslationAsync()
    {
        ProgressValue = 0;
        downloadService = null;

        try
        {
            if (ListViewSelectedItem != null)
            {
                var translationItem = ListViewSelectedItem as QuranTranslation;

                if (downloadService is not null && downloadService.IsCancelled)
                {
                    return;
                }
                else
                {
                    downloadService = new DownloadService();
                    downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    await downloadService.DownloadFileTaskAsync(translationItem.Link, Path.Combine(Path.GetTempPath(), translationItem.TranslationId + ".txt"));
                }
            }
            else
            {
                IsDownloadActive = true;
                IsCancelActive = false;
            }
        }
        catch (Exception)
        {
            IsDownloadActive = true;
            IsCancelActive = false;
        }
    }
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            DownloadPackage pack = (DownloadPackage) e.UserState;
            IsDownloadActive = true;
            IsCancelActive = false;
            ProgressValue = 0;
            AddIntoDatabase(pack.FileName);
        });
    }

    private async void AddIntoDatabase(string fileName)
    {
        var data = ExtractData(fileName);
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            var translation = new TranslationsText
            {
                Text = json,
                TranslationId = Path.GetFileNameWithoutExtension(fileName)
            };
            using var db = new AlAnvarDBContext();
            await db.TranslationsText.AddAsync(translation);
            var updateTranslation = await db.Translations.Where(x => x.TranslationId.Equals(Path.GetFileNameWithoutExtension(fileName))).FirstOrDefaultAsync();
            updateTranslation.IsActive = true;
            await db.SaveChangesAsync();
        }
    }
    public class DataEntry
    {
        public int SurahId { get; set; }
        public int Aya { get; set; }
        public string Translation { get; set; }
    }

    private List<DataEntry> ExtractData(string fileName)
    {
        List<DataEntry> dataEntries = new List<DataEntry>();
        using (FileStream fileStream = File.OpenRead(fileName))
        using (StreamReader reader = new StreamReader(fileStream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Skip blank lines and lines starting with #
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split('|');
                if (parts.Length == 3)
                {
                    if (int.TryParse(parts[0], out int number1) &&
                        int.TryParse(parts[1], out int number2))
                    {
                        DataEntry dataEntry = new DataEntry
                        {
                            SurahId = number1,
                            Aya = number2,
                            Translation = parts[2]
                        };
                        dataEntries.Add(dataEntry);
                    }
                }
            }
        }
        return dataEntries;
    }

    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ProgressValue = e.ProgressPercentage;
        });
    }
}
