namespace AlAnvar.Database.Tables;

[Table("SubjectName")]
public class SubjectName
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public long SubjectId { get; set; }
    public string Name { get; set; }
    public long ParentId { get; set; }
    public int Type { get; set; }
    public int Ordering { get; set; }
    public string Date { get; set; }
}
