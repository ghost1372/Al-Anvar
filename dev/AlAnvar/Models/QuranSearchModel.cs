namespace AlAnvar.Models;

public partial class QuranSearchModel
{
    public int Id { get; set; }
    public int SuraId { get; set; }
    public int AyaId { get; set; }

    public string SuraName { get; set; }
    public string SuraEnglishName { get; set; }
    public string SuraFinglishName { get; set; }

    public string Aya { get; set; }
    public string CleanAya { get; set; }

    public string Translation { get; set; }
}
