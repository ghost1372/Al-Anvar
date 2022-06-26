namespace AlAnvar.Database.Tables;

[Table("Subjects")]
public class Subjects
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public long SubjectId { get; set; }
    public int VerseId { get; set; }
}
