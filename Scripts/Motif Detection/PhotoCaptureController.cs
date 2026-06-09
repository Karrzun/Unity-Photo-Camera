using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoCaptureController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhotoDatabaseSO database;
    [SerializeField] private CameraMotifDetector motifDetector;

    private AlbumProgressService albumProgressService;

    private void Awake()
    {
        AlbumProgress progress = new AlbumProgress();
        albumProgressService = new AlbumProgressService(database, progress);

        if (motifDetector == null)
            motifDetector = GetComponent<CameraMotifDetector>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            CapturePhoto();
        }

        if (keyboard.rKey.wasPressedThisFrame && motifDetector != null)
        {
            motifDetector.RefreshMotifsInScene();
            Debug.Log("Motifs in scene refreshed.", motifDetector);
        }
    }

    private void CapturePhoto()
    {
        if (motifDetector == null)
        {
            Debug.LogWarning("No CameraMotifDetector assigned.", this);
            return;
        }

        MotifDetectionResult detectionResult = motifDetector.CurrentResult;

        if (!detectionResult.IsValid)
        {
            Debug.Log(
                $"Photo captured, but current composition is invalid.\n" +
                $"Detection Status: {detectionResult.Status}",
                this
            );

            return;
        }

        CollectMotifResult collectResult =
            albumProgressService.Collect(detectionResult.Motif);

        Debug.Log(
            $"Photo captured.\n" +
            $"Detection Status: {detectionResult.Status}\n" +
            $"{CollectMotifResultFormatter.Format(collectResult)}",
            detectionResult.MotifIdentity
        );
    }
}