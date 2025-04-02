using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Agent
{
    public const int sizeOf = 16;

    public Vector2 position;
    public Vector2 forward;

    public Agent(Vector2 position, Vector2 forward)
    {
        this.position = position;
        this.forward = forward;
    }
}
