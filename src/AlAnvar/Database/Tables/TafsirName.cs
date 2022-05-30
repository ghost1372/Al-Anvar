namespace AlAnvar.Database.Tables;

[Table("TafsirName")]
public class TafsirName
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
}
