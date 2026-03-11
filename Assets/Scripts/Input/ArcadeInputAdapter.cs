using UnityEngine;
using System;

public class ArcadeInputAdapter : MonoBehaviour, IInputAdapter
{
    public float MoveHorizontal { get; private set; }

    public event Action OnJumpPressed;

    void Update()
    {
#if LUXODD_PLUGIN
        StickData stick = ArcadeControls.GetStick();
        MoveHorizontal = stick.Vector.x;

        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Black))
            OnJumpPressed?.Invoke();
#else
        MoveHorizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
            OnJumpPressed?.Invoke();
#endif
    }
}
