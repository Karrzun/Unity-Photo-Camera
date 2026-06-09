using System.Collections.Generic;
using System.Linq;

public class AlbumProgressService
{
    private readonly PhotoDatabaseSO database;
    private readonly AlbumProgress progress;

    public AlbumProgressService(PhotoDatabaseSO database, AlbumProgress progress)
    {
        this.database = database;
        this.progress = progress;
    }

    public CollectMotifResult Collect(MotifDefinitionSO motif)
    {
        if (motif == null)
            return CollectMotifResult.Invalid(motif);

        List<AlbumDefinitionSO> affectedAlbums = database.GetAlbumsContaining(motif).ToList();

        if (affectedAlbums.Count == 0)
            return CollectMotifResult.NoAlbumFound(motif);

        bool wasNewlyCollected = progress.MarkCollected(motif);

        if (!wasNewlyCollected)
            return CollectMotifResult.AlreadyCollected(motif, affectedAlbums);

        List<AlbumDefinitionSO> newlyCompletedAlbums = affectedAlbums
            .Where(IsAlbumCompleted)
            .Where(album => !progress.IsAlbumCompleted(album))
            .ToList();

        foreach (AlbumDefinitionSO album in newlyCompletedAlbums)
        {
            progress.MarkAlbumCompleted(album);
        }

        if (newlyCompletedAlbums.Count > 0)
        {
            return CollectMotifResult.AlbumCompleted(motif, affectedAlbums, newlyCompletedAlbums);
        }

        return CollectMotifResult.NewlyCollected(motif, affectedAlbums);
    }

    public bool IsAlbumCompleted(AlbumDefinitionSO album)
    {
        if (album == null || album.Motifs.Count == 0)
            return false;

        return album.Motifs
            .Where(motif => motif != null)
            .All(progress.IsCollected);
    }
}