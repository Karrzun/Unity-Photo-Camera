using UnityEngine;

[CreateAssetMenu(fileName = "MotifDetectionOptions", menuName = "Photo Camera/Detection Options")]
public class MotifDetectionOptionsSO : ScriptableObject
{
    [Header("Visibility")]
    [SerializeField] private float maxCenterOffset = 0.2f;

    [Header("Distance / Framing")]
    [SerializeField] private MotifFramingPlanes motifFramingPlanes = new MotifFramingPlanes(1000f, 2000f);

    [Header("Occlusion")]
    [SerializeField] private float sampleSpread = 3f;
    [SerializeField] private int sampleCount = 12;
    [SerializeField] private int occlusionThreshold = 4;

    public float MaxCenterOffset => maxCenterOffset;
    public MotifFramingPlanes MotifFramingPlanes => motifFramingPlanes;
    public float SampleSpread => sampleSpread;
    public int SampleCount => sampleCount;
    public int OcclusionThreshold => occlusionThreshold;
}