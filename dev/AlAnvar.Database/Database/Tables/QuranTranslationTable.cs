using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlAnvar.Database.Tables;

[Table("Translations")]
public partial class QuranTranslationTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string TranslationId { get; set; }
    public string Language { get; set; }
    public string Name { get; set; }
    public string Translator { get; set; }
    public string Link { get; set; }
    public string OriginalLink { get; set; }
    public bool IsActive { get; set; }
}
