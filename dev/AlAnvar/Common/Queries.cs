using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlAnvar.Database;

public partial class Queries
{
    public static readonly Func<AlAnvarDBContext, int, IAsyncEnumerable<QuranTafsirTable>> GetTafsirByIdQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context, int explanationId) => context.QuranTafsirs.Where(x => x.ExplanationId == explanationId));

    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranTafsirNameTable>> GetAllTafsirNamesQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context) => context.QuranTafsirNames);

    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranNoteTable>> GetAllNotesQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context) => context.Notes);

    public static readonly Func<AlAnvarDBContext, int, IAsyncEnumerable<QuranNoteTable>> GetNoteByIdQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context, int noteId) => context.Notes.Where(x => x.Id == noteId));

    public static readonly Func<AlAnvarDBContext, IReadOnlyCollection<int>, IAsyncEnumerable<QuranNoteTable>> GetNotesByIdsAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context, IReadOnlyCollection<int> noteIds) => context.Notes.Where(x => noteIds.Contains(x.Id)));


    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranMetadataTable>> GetAllChaptersQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context) => context.Chapters);

    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranTable>> GetQuranQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context) => context.Qurans);

    public static readonly Func<AlAnvarDBContext, int, int, IAsyncEnumerable<QuranTable>> GetQuranByIdsQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context, int suraId, int verseId) => context.Qurans.Where(x => x.SuraId == suraId && x.AyaId == verseId));

    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranCleanTable>> GetQuranCleanQueryAsync =
       EF.CompileAsyncQuery((AlAnvarDBContext context) => context.QuransClean);

    public static readonly Func<AlAnvarDBContext, int, IAsyncEnumerable<FinalQuran>> GetQuranWithCleanBySuraQueryAsync =
        EF.CompileAsyncQuery((AlAnvarDBContext context, int suraId) =>
        from q in context.Qurans
        where q.SuraId == suraId
        join qc in context.QuransClean
            on new { q.SuraId, q.AyaId } equals new { qc.SuraId, qc.AyaId } into cleanJoin
        from qc in cleanJoin.DefaultIfEmpty()
        orderby q.AyaId
        select new FinalQuran
        {
            Id = q.Id,
            SuraId = q.SuraId,
            AyaId = q.AyaId,
            Aya = q.Aya,
            CleanAya = qc.Aya,
            // These will be set outside the query since EF can't access quranMetadataTable here
            SuraName = "",
            SuraFinglishName = "",
            JuzId = q.JuzId,
            HizbId = q.HizbId,
            AudioFileName = q.AudioFileName
        });

    public static readonly Func<AlAnvarDBContext, IAsyncEnumerable<QuranSearchModel>> GetQuranSearchQueryAsync =
        EF.CompileAsyncQuery((AlAnvarDBContext context) =>
        from q in context.Qurans
        join m in context.Chapters
        on q.SuraId equals m.Id
        join c in context.QuransClean
        on new { q.SuraId, q.AyaId }
        equals new { c.SuraId, c.AyaId }
        into cleanJoin
        from clean in cleanJoin.DefaultIfEmpty()
        select new QuranSearchModel
        {
            Id = q.Id,
            SuraId = q.SuraId,
            AyaId = q.AyaId,

            SuraName = m.Name,
            SuraEnglishName = m.EnglishName,
            SuraFinglishName = m.FinglishName,

            Aya = q.Aya,
            CleanAya = clean != null ? clean.Aya : "",
            Translation = ""
        });
}
