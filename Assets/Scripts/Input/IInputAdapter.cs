using System;
using UnityEngine;

public interface IInputAdapter
{
    float MoveHorizontal { get; }
    bool IsFireHeld { get; }
    Vector2 AimDirection { get; }
    event Action OnJumpPressed;
}
