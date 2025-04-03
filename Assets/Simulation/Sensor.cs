using System;

[Serializable]
public struct Sensor
{
    public const int sizeOf = 8;

    public float angle;
    public float distance;

    public Sensor(float angle, float distance)
    {
        this.angle = angle;
        this.distance = distance;
    }
}
