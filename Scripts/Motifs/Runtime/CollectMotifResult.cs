using System.Collections.Generic;
using System.Linq;

public readonly struct CollectMotifResult
{
    public readonly CollectMotifStatus Status;
    public readonly MotifDefinitionSO Motif;
    public readonly IReadOnlyList<AlbumDefinitionSO> AffectedAlbums;
    public readonly IReadOnlyList<AlbumDefinitionSO> CompletedAlbums;
    public readonly int MotifReward;
    public readonly int AlbumCompletionReward;

    public int TotalReward => MotifReward + AlbumCompletionReward;

    public bool WasCollectedSuccessfully =>
        Status == CollectMotifStatus.NewlyCollected ||
        Status == CollectMotifStatus.AlbumCompleted;

    public bool CompletedAnyAlbum => CompletedAlbums != null && CompletedAlbums.Count > 0;

    private CollectMotifResult(
        CollectMotifStatus status,
        MotifDefinitionSO motif,
        IReadOnlyList<AlbumDefinitionSO> affectedAlbums,
        IReadOnlyList<AlbumDefinitionSO> completedAlbums,
        int motifReward,
        int albumCompletionReward)
    {
        Status = status;
        Motif = motif;
        AffectedAlbums = affectedAlbums;
        CompletedAlbums = completedAlbums;
        MotifReward = motifReward;
        AlbumCompletionReward = albumCompletionReward;
    }

    public static CollectMotifResult Invalid(MotifDefinitionSO motif = null)
    {
        return new CollectMotifResult(
            CollectMotifStatus.Invalid,
            motif,
            new List<AlbumDefinitionSO>(),
            new List<AlbumDefinitionSO>(),
            0,
            0
        );
    }

    public static CollectMotifResult NoAlbumFound(MotifDefinitionSO motif)
    {
        return new CollectMotifResult(
            CollectMotifStatus.NoAlbumFound,
            motif,
            new List<AlbumDefinitionSO>(),
            new List<AlbumDefinitionSO>(),
            0,
            0
        );
    }

    public static CollectMotifResult AlreadyCollected(MotifDefinitionSO motif, IReadOnlyList<AlbumDefinitionSO> affectedAlbums)
    {
        return new CollectMotifResult(
            CollectMotifStatus.AlreadyCollected,
            motif,
            affectedAlbums,
            new List<AlbumDefinitionSO>(),
            0,
            0
        );
    }

    public static CollectMotifResult NewlyCollected(MotifDefinitionSO motif, IReadOnlyList<AlbumDefinitionSO> affectedAlbums)
    {
        return new CollectMotifResult(
            CollectMotifStatus.NewlyCollected,
            motif,
            affectedAlbums,
            new List<AlbumDefinitionSO>(),
            motif.RewardAmount,
            0
        );
    }

    public static CollectMotifResult AlbumCompleted(MotifDefinitionSO motif, IReadOnlyList<AlbumDefinitionSO> affectedAlbums, IReadOnlyList<AlbumDefinitionSO> completedAlbums)
    {
        int completionReward = completedAlbums.Sum(album => album.CompletionReward);

        return new CollectMotifResult(
            CollectMotifStatus.AlbumCompleted,
            motif,
            affectedAlbums,
            completedAlbums,
            motif.RewardAmount,
            completionReward
        );
    }
}