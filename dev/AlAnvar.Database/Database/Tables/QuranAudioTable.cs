using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlAnvar.Database.Tables;

[Table("Audio")]
public partial class QuranAudioTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string PersianName { get; set; }
    public string DirName { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
}
