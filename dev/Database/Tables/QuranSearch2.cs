namespace AlAnvar.Database.Tables;

public class QuranSearch2
{
    public int Id { get; set; }
    public int SurahId { get; set; }
    public int AyahNumber { get; set; }
    public string SurahName { get; set; }
    public string Text { get; set; }
    public bool IsTranslation { get; set; }
}
