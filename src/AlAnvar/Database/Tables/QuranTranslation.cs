namespace AlAnvar.Database.Tables;

[Table("Translations")]
public class QuranTranslation
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string TranslationId { get; set; }
    public string Language { get; set; }
    public string Name { get; set; }
    public string Translator { get; set; }
    public string Link { get; set; }
}
