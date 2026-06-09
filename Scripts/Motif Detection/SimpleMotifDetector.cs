using UnityEngine;

public class SimpleMotifDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera detectionCamera;

    [Header("Detection")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask detectionMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    public bool TryDetectMotif(out MotifIdentity motifIdentity)
    {
        motifIdentity = null;

        if (detectionCamera == null)
        {
            Debug.LogWarning("No detection camera assigned.", this);
            return false;
        }

        Ray ray = detectionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (drawDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 0.1f);
        }

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, detectionMask))
        {
            return false;
        }

        motifIdentity = hit.collider.GetComponentInParent<MotifIdentity>();

        if (motifIdentity == null)
        {
            return false;
        }

        return motifIdentity.HasMotif;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (detectionCamera == null)
        {
            detectionCamera = Camera.main;
        }
    }
#endif
}