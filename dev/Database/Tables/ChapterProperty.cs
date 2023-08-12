namespace AlAnvar.Database.Tables;

[Table("ChapterProperty")]
public class ChapterProperty
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int Ayas { get; set; }
    public int Start { get; set; }
    public string Name { get; set; }
    public string TName { get; set; }
    public string EName { get; set; }
    public string Type { get; set; }
    public int Order { get; set; }
    public int Rukus { get; set; }
}
