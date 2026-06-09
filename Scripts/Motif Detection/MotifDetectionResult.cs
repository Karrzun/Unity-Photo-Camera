public readonly struct MotifDetectionResult
{
    public readonly MotifStatus Status;
    public readonly MotifIdentity MotifIdentity;

    public MotifDefinitionSO Motif => MotifIdentity != null
        ? MotifIdentity.Motif
        : null;

    public bool IsValid => Status == MotifStatus.Valid && MotifIdentity != null;

    public MotifDetectionResult(MotifStatus status, MotifIdentity motifIdentity)
    {
        Status = status;
        MotifIdentity = motifIdentity;
    }

    public static MotifDetectionResult Invalid()
    {
        return new MotifDetectionResult(MotifStatus.Invalid, null);
    }
}