using System.Collections.Generic;

public class AlbumProgress
{
    private readonly HashSet<string> collectedMotifIds = new();
    private readonly HashSet<string> completedAlbumIds = new();

    public bool IsCollected(MotifDefinitionSO motif)
    {
        return motif != null && collectedMotifIds.Contains(motif.Id);
    }

    public bool MarkCollected(MotifDefinitionSO motif)
    {
        if (motif == null)
            return false;

        return collectedMotifIds.Add(motif.Id);
    }

    public bool IsAlbumCompleted(AlbumDefinitionSO album)
    {
        return album != null && completedAlbumIds.Contains(album.Id);
    }

    public bool MarkAlbumCompleted(AlbumDefinitionSO album)
    {
        if (album == null)
            return false;

        return completedAlbumIds.Add(album.Id);
    }
}