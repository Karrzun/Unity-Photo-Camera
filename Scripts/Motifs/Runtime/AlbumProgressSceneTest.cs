using UnityEngine;
using UnityEngine.InputSystem;

public class AlbumProgressSceneTest : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private PhotoDatabaseSO database;

    [Header("Test Scene Motifs")]
    [SerializeField] private MotifIdentity brownBearIdentity;
    [SerializeField] private MotifIdentity polarBearIdentity;
    [SerializeField] private MotifIdentity elephantIdentity;

    private AlbumProgressService albumProgressService;

    private void Awake()
    {
        AlbumProgress progress = new AlbumProgress();
        albumProgressService = new AlbumProgressService(database, progress);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            CollectFromIdentity(brownBearIdentity);
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            CollectFromIdentity(polarBearIdentity);
        }

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            CollectFromIdentity(elephantIdentity);
        }
    }

    private void CollectFromIdentity(MotifIdentity identity)
    {
        if (identity == null)
        {
            Debug.LogWarning("No MotifIdentity assigned.");
            return;
        }

        if (!identity.HasMotif)
        {
            Debug.LogWarning($"MotifIdentity on '{identity.gameObject.name}' has no MotifDefinitionSO assigned.", identity);
            return;
        }

        CollectMotifResult result = albumProgressService.Collect(identity.Motif);

        Debug.Log(BuildResultMessage(result), identity);
    }

    private string BuildResultMessage(CollectMotifResult result)
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
            $"Collected Motif: {motifName}\n" +
            $"Status: {result.Status}\n" +
            $"Affected Albums: {affectedAlbums}\n" +
            $"Completed Albums: {completedAlbums}\n" +
            $"Motif Reward: {result.MotifReward}\n" +
            $"Album Completion Reward: {result.AlbumCompletionReward}\n" +
            $"Total Reward: {result.TotalReward}";
    }

    private string[] GetAlbumNames(System.Collections.Generic.IReadOnlyList<AlbumDefinitionSO> albums)
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