using System.Collections.Generic;

public static class CollectMotifResultFormatter
{
    public static string Format(CollectMotifResult result)
    {
        string motifName = result.Motif != null
            ? result.Motif.DisplayName
            : "None";

        string affectedAlbums = result.AffectedAlbums != null && result.AffectedAlbums.Count > 0
            ? string.Join(", ", GetAlbumNames(result.AffectedAlbums))
            : "None";

        string completedAlbums = result.CompletedAlbums != null && result.CompletedAlbums.Count > 0
            ? string.Join(", ", GetAlbumNames(result.CompletedAlbums))
            : "None";

        return
            $"Detected Motif: {motifName}\n" +
            $"Status: {result.Status}\n" +
            $"Affected Albums: {affectedAlbums}\n" +
            $"Completed Albums: {completedAlbums}\n" +
            $"Motif Reward: {result.MotifReward}\n" +
            $"Album Completion Reward: {result.AlbumCompletionReward}\n" +
            $"Total Reward: {result.TotalReward}";
    }

    private static string[] GetAlbumNames(IReadOnlyList<AlbumDefinitionSO> albums)
    {
        string[] names = new string[albums.Count];

        for (int i = 0; i < albums.Count; i++)
        {
            names[i] = albums[i] != null
                ? albums[i].DisplayName
                : "Null";
        }

        return names;
    }
}