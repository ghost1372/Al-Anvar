using AlAnvar.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace AlAnvar.Database;

public partial class AlAnvarDBContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string filename = Constants.DatabaseFilePath;
        if (!string.IsNullOrEmpty(Settings.DBPath))
        {
            filename = $"{Settings.DBPath}";
        }
        optionsBuilder.UseSqlite($"Data Source={filename}");
    }

    public DbSet<QuranMetadataTable> Chapters { get; set; }
    public DbSet<QuranTable> Qurans { get; set; }
    public DbSet<QuranCleanTable> QuransClean { get; set; }
    public DbSet<QuranTranslationTable> Translations { get; set; }
    public DbSet<QuranAudioTable> Audios { get; set; }
    public DbSet<QuranFavoriteTable> Favorites { get; set; }
    public DbSet<QuranNoteTable> Notes { get; set; }
    public DbSet<QuranTafsirNameTable> QuranTafsirNames { get; set; }
    public DbSet<QuranTafsirTable> QuranTafsirs { get; set; }
}
