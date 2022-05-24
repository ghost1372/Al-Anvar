namespace AlAnvar.Database.Tables;

[Table("Audio")]
public class QuranAudio
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string PName { get; set; }
    public string DirName { get; set; }
    public string Url { get; set; }
}
