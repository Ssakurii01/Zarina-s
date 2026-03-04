using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public IInputAdapter Input { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

#if LUXODD_PLUGIN
        Input = gameObject.AddComponent<ArcadeInputAdapter>();
#else
        Input = gameObject.AddComponent<KeyboardInputAdapter>();
#endif
    }
}
