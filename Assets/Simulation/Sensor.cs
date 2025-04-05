using System;

[Serializable]
public struct Sensor
{
    public const int sizeOf = 16;

    public float yaw;
    public float pitch;
    public float distance;
    public float turnSpeed;

    public Sensor(float yaw, float pitch, float distance, float turnSpeed)
    {
        this.yaw = yaw;
        this.pitch = pitch;
        this.distance = distance;
        this.turnSpeed = turnSpeed;
    }

    public override bool Equals(object obj)
    {
        return obj is Sensor sensor &&
               yaw == sensor.yaw &&
               pitch == sensor.pitch &&
               distance == sensor.distance &&
               turnSpeed == sensor.turnSpeed;
    }
}
