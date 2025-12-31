using System.ComponentModel;

namespace AlAnvar.Models;
public partial class FinalQuran : ObservableObject
{    
    public int Id { get; set; }

    public int SuraId { get; set; }

    public int AyaId { get; set; }

    public int JuzId { get; set; }

    public int HizbId { get; set; }

    public string Aya { get; set; }

    public string CleanAya { get; set; }

    public string SuraName { get; set; }

    public string SuraFinglishName { get; set; }

    public string Translation { get; set; }

    public string AudioFileName { get; set; }

    public bool IsFavorite { get; set; }

    public string DisplayAya => ProxyService.Instance.IsDiacriticsVisible ? Aya : CleanAya;

    public FinalQuran()
    {
        ProxyService.Instance.PropertyChanged -= OnPropertyChanged;
        ProxyService.Instance.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProxyService.IsDiacriticsVisible))
        {
            OnPropertyChanged(nameof(DisplayAya));
        }
    }
}
