using System;

public interface IInputAdapter
{
    float MoveHorizontal { get; }
    event Action OnJumpPressed;
}
