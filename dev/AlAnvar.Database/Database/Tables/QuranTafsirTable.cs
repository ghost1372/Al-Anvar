using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlAnvar.Database.Tables;

[Table("Explanation")]
public partial class QuranTafsirTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int ExplanationId { get; set; }
    public string VerseIds { get; set; }
    public string Description { get; set; }
}
