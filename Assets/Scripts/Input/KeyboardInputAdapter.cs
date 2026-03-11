using UnityEngine;
using System;

public class KeyboardInputAdapter : MonoBehaviour, IInputAdapter
{
    public float MoveHorizontal { get; private set; }

    public event Action OnJumpPressed;

    void Update()
    {
        MoveHorizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnJumpPressed?.Invoke();
        }
    }
}
