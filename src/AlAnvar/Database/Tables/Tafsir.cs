namespace AlAnvar.Database.Tables;

[Table("Tafsir")]
public class Tafsir
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int IdName { get; set; }
    public string IdVerse { get; set; }
    public string Value { get; set; }
}
