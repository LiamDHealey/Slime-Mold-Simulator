using System;

[Serializable]
public struct Sensor
{
    public const int sizeOf = 20;

    public float yaw;
    public float pitch;
    public float distance;
    public float yawTurnSpeed;
    public float pitchTurnSpeed;

    public Sensor(float yaw, float pitch, float distance, float yawTurnSpeed, float pitchTurnSpeed)
    {
        this.yaw = yaw;
        this.pitch = pitch;
        this.distance = distance;
        this.yawTurnSpeed = yawTurnSpeed;
        this.pitchTurnSpeed = pitchTurnSpeed;
    }

    public override bool Equals(object obj)
    {
        return obj is Sensor sensor &&
               yaw == sensor.yaw &&
               pitch == sensor.pitch &&
               distance == sensor.distance &&
               yawTurnSpeed == sensor.yawTurnSpeed &&
               pitchTurnSpeed == sensor.pitchTurnSpeed;
    }
}
