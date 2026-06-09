using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMotifRaycastTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera detectionCamera;
    [SerializeField] private PhotoDatabaseSO database;

    [Header("Detection")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask detectionMask = ~0;

    private AlbumProgressService albumProgressService;

    private void Awake()
    {
        if (detectionCamera == null)
            detectionCamera = Camera.main;

        AlbumProgress progress = new AlbumProgress();
        albumProgressService = new AlbumProgressService(database, progress);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.fKey.wasPressedThisFrame)
        {
            TryCollectMotifInCenter();
        }
    }

    private void TryCollectMotifInCenter()
    {
        if (detectionCamera == null)
        {
            Debug.LogWarning("No detection camera assigned.");
            return;
        }

        Ray ray = detectionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, detectionMask))
        {
            Debug.Log("No object hit.");
            return;
        }

        if (!hit.collider.TryGetComponent(out MotifIdentity motifIdentity))
        {
            Debug.Log($"Hit '{hit.collider.gameObject.name}', but it has no MotifIdentity.", hit.collider);
            return;
        }

        if (!motifIdentity.HasMotif)
        {
            Debug.LogWarning($"'{motifIdentity.gameObject.name}' has MotifIdentity but no MotifDefinitionSO assigned.", motifIdentity);
            return;
        }

        CollectMotifResult result = albumProgressService.Collect(motifIdentity.Motif);

        Debug.Log(BuildResultMessage(result), motifIdentity);
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