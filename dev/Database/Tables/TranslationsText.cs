namespace AlAnvar.Database.Tables;

[Table("TranslationsText")]
public class TranslationsText
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string TranslationId { get; set; }
    public string Text { get; set; }
}
