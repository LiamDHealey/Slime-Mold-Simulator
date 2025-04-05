using UnityEngine;

public struct Agent
{
    public const int sizeOf = 20;

    public Vector3 position;
    public float yaw;
    public float pitch;

    public Agent(Vector3 position, float yaw, float pitch)
    {
        this.position = position;
        this.yaw = yaw;
        this.pitch = pitch;
    }
}
