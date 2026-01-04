using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlAnvar.Database.Tables;

[Table("ChapterProperty")]
public partial class QuranMetadataTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Aya { get; set; }
    public int Start { get; set; }
    public string Name { get; set; }
    public string FinglishName { get; set; }
    public string EnglishName { get; set; }
    public string Type { get; set; }
    public int Order { get; set; }
    public int Rukus { get; set; }
}
