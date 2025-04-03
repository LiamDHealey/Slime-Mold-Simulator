using UnityEngine;

public struct Agent
{
    public const int sizeOf = 12;

    public Vector2 position;
    public float angle;

    public Agent(Vector2 position, float angle)
    {
        this.position = position;
        this.angle = angle;
    }
}
