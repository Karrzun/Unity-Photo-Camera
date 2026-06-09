using System;

[Serializable]
public struct MotifFramingPlanes
{
    public float Near;
    public float Far;

    public MotifFramingPlanes(float near, float far)
    {
        Near = near;
        Far = far;
    }
}