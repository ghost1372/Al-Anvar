namespace AlAnvar.Database.Tables;

public class QuranItem
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int SurahId { get; set; }
    public int AyahNumber { get; set; }
    public int TotalAyah { get; set; }
    public string AyahText { get; set; }
    public string TranslationText { get; set; }
    public string AyaDetail { get; set; }
    public int Juz { get; set; }
    public int Hizb { get; set; }
    public string Audio { get; set; }
}
