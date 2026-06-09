using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraMotifDetector : MonoBehaviour
{
    public event Action<MotifDetectionResult> DetectionResultChanged;

    [Header("References")]
    [SerializeField] private Camera detectionCamera;
    [SerializeField] private MotifDetectionOptionsSO options;

    [Header("Detection")]
    [SerializeField] private LayerMask occlusionMask = ~0;
    [SerializeField] private bool scanContinuously = true;
    [SerializeField] private float scanInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRays = true;
    [SerializeField] private bool logStatusChanges = false;

    private readonly List<MotifIdentity> motifsInScene = new();
    private readonly List<MotifIdentity> visibleMotifs = new();
    private readonly Dictionary<MotifIdentity, float> lateralOffsetByMotif = new();
    private readonly Dictionary<MotifIdentity, float> priorityByMotif = new();
    private readonly List<Vector3> samples = new();

    private float nextScanTime;

    public MotifDetectionResult CurrentResult { get; private set; } = MotifDetectionResult.Invalid();

    public bool HasValidMotif => CurrentResult.IsValid;

    private void Awake()
    {
        if (detectionCamera == null)
            detectionCamera = Camera.main;

        RefreshMotifsInScene();
        CurrentResult = MotifDetectionResult.Invalid();
    }

    private void Update()
    {
        if (!scanContinuously)
            return;

        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;
        Scan();
    }

    public void RefreshMotifsInScene()
    {
        motifsInScene.Clear();

#if UNITY_2023_1_OR_NEWER
        motifsInScene.AddRange(FindObjectsByType<MotifIdentity>(FindObjectsSortMode.None));
#else
        motifsInScene.AddRange(FindObjectsOfType<MotifIdentity>());
#endif

        motifsInScene.RemoveAll(identity => identity == null || !identity.HasMotif);
    }

    public MotifDetectionResult Scan()
    {
        MotifDetectionResult newResult = DetectInternal();

        bool changed = newResult.Status != CurrentResult.Status || newResult.MotifIdentity != CurrentResult.MotifIdentity;
        CurrentResult = newResult;

        if (changed)
        {
            DetectionResultChanged?.Invoke(CurrentResult);

            if (logStatusChanges)
            {
                Debug.Log($"Motif detection changed: {CurrentResult.Status}", this);
            }
        }

        return CurrentResult;
    }

    public bool TryGetCurrentMotif(out MotifIdentity motifIdentity)
    {
        motifIdentity = CurrentResult.MotifIdentity;
        return CurrentResult.IsValid;
    }

    private MotifDetectionResult DetectInternal()
    {
        PrepareScanningProcess();

        if (detectionCamera == null)
        {
            Debug.LogWarning("No detection camera assigned.", this);
            return MotifDetectionResult.Invalid();
        }

        if (options == null)
        {
            Debug.LogWarning("No MotifDetectionOptionsSO assigned.", this);
            return MotifDetectionResult.Invalid();
        }

        if (CountVisibleMotifs() == 0)
            return MotifDetectionResult.Invalid();

        EvaluateMotifPriorityByPosition();

        MotifIdentity bestMotif = SelectBestMotif();

        if (bestMotif == null)
            return MotifDetectionResult.Invalid();

        MotifStatus compositionStatus = EvaluatePictureComposition(bestMotif);

        if (compositionStatus != MotifStatus.Valid)
            return new MotifDetectionResult(compositionStatus, bestMotif);

        if (IsMotifOccluded(bestMotif))
            return new MotifDetectionResult(MotifStatus.MotifOccluded, bestMotif);

        return new MotifDetectionResult(MotifStatus.Valid, bestMotif);
    }

    private void PrepareScanningProcess()
    {
        visibleMotifs.Clear();
        lateralOffsetByMotif.Clear();
        priorityByMotif.Clear();
        samples.Clear();
    }

    private int CountVisibleMotifs()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(detectionCamera);

        foreach (MotifIdentity motifIdentity in motifsInScene)
        {
            if (motifIdentity == null || !motifIdentity.HasMotif)
                continue;

            if (!TryGetWorldBounds(motifIdentity, out Bounds bounds))
                continue;

            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
                continue;

            visibleMotifs.Add(motifIdentity);
        }

        return visibleMotifs.Count;
    }

    private void EvaluateMotifPriorityByPosition()
    {
        foreach (MotifIdentity motifIdentity in visibleMotifs)
        {
            Vector3 targetPosition = GetTargetPosition(motifIdentity);
            Vector3 viewportPosition = detectionCamera.WorldToViewportPoint(targetPosition);

            float xOffset = viewportPosition.x - 0.5f;
            float yOffset = viewportPosition.y - 0.5f;
            float lateralOffset = Mathf.Sqrt(xOffset * xOffset + yOffset * yOffset);

            float depthValue = ((viewportPosition.z + 1f) * detectionCamera.fieldOfView) / 1000f;
            float depthOffset = Mathf.Pow(depthValue, 2f);

            lateralOffsetByMotif[motifIdentity] = lateralOffset;
            priorityByMotif[motifIdentity] = (14f * lateralOffset + depthOffset) / 15f;
        }
    }

    private MotifIdentity SelectBestMotif()
    {
        if (priorityByMotif.Count == 0)
            return null;

        return priorityByMotif.OrderBy(entry => entry.Value).First().Key;
    }

    private MotifStatus EvaluatePictureComposition(MotifIdentity motifIdentity)
    {
        Vector3 targetPosition = GetTargetPosition(motifIdentity);

        float relativeDistance = Vector3.Distance(targetPosition, detectionCamera.transform.position) * detectionCamera.fieldOfView;

        if (lateralOffsetByMotif.TryGetValue(motifIdentity, out float lateralOffset))
        {
            if (lateralOffset > options.MaxCenterOffset)
                return MotifStatus.BadFraming;
        }

        if (relativeDistance > options.MotifFramingPlanes.Far)
            return MotifStatus.TooFar;

        if (relativeDistance < options.MotifFramingPlanes.Near)
            return MotifStatus.TooClose;

        return MotifStatus.Valid;
    }

    private bool IsMotifOccluded(MotifIdentity motifIdentity)
    {
        Vector3 targetPosition = GetTargetPosition(motifIdentity);
        Vector3 origin = detectionCamera.transform.position;
        float distance = Vector3.Distance(targetPosition, origin);
        Vector3 direction = (targetPosition - origin).normalized;

        samples.Clear();

        for (int i = 0; i < options.SampleCount; i++)
        {
            SampleRaycastDirection(i, direction, detectionCamera.fieldOfView * (options.SampleSpread / 100f));
        }

        int occlusionCounter = 0;

        foreach (Vector3 sampleDirection in samples)
        {
            if (drawDebugRays)
                Debug.DrawRay(origin, sampleDirection * distance, Color.red, scanInterval);

            if (!Physics.Raycast(origin, sampleDirection, out RaycastHit hitInfo, distance, occlusionMask))
                continue;

            MotifIdentity hitMotifIdentity = hitInfo.collider.GetComponentInParent<MotifIdentity>();

            if (hitMotifIdentity != motifIdentity)
                occlusionCounter++;
        }

        return occlusionCounter > options.OcclusionThreshold;
    }

    private void SampleRaycastDirection(int index, Vector3 direction, float angle)
    {
        float radians = (2f * Mathf.PI / options.SampleCount) * (index + 1);

        float xOffset = Mathf.Cos(radians) * angle;
        float yOffset = Mathf.Sin(radians) * angle;

        Vector3 rotatedAroundUp = Quaternion.AngleAxis(xOffset, detectionCamera.transform.up) * direction;
        Vector3 sampleDirection = Quaternion.AngleAxis(yOffset, detectionCamera.transform.right) * rotatedAroundUp;

        samples.Add(sampleDirection.normalized);
    }

    private Vector3 GetTargetPosition(MotifIdentity motifIdentity)
    {
        if (TryGetWorldBounds(motifIdentity, out Bounds bounds))
            return bounds.center;

        return motifIdentity.transform.position;
    }

    private bool TryGetWorldBounds(MotifIdentity motifIdentity, out Bounds bounds)
    {
        bounds = default;

        Collider[] colliders = motifIdentity.GetComponentsInChildren<Collider>();

        bool hasBounds = false;

        foreach (Collider collider in colliders)
        {
            if (collider == null || !collider.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (detectionCamera == null)
            detectionCamera = Camera.main;

        scanInterval = Mathf.Max(0.02f, scanInterval);
    }
#endif
}