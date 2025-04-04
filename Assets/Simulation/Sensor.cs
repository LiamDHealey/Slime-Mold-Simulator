using System;

[Serializable]
public struct Sensor
{
    public const int sizeOf = 12;

    public float angle;
    public float distance;
    public float turnSpeed;

    public Sensor(float angle, float distance, float turnSpeed)
    {
        this.angle = angle;
        this.distance = distance;
        this.turnSpeed = turnSpeed;
    }

    public override bool Equals(object obj)
    {
        return obj is Sensor sensor &&
               angle == sensor.angle &&
               distance == sensor.distance &&
               turnSpeed == sensor.turnSpeed;
    }
}
