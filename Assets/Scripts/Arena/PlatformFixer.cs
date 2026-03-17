using UnityEngine;

/// <summary>
/// Auto-runs at scene load. Finds all platforms and repositions them to be centered and reachable.
/// No need to attach to a GameObject - uses RuntimeInitializeOnLoadMethod.
/// </summary>
public static class PlatformFixer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void FixPlatforms()
    {
        var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (var t in allTransforms)
        {
            if (t == null) continue;
            string name = t.gameObject.name.ToLower();

            // Skip non-platform objects
            if (name.Contains("arena") || name.Contains("wall") || name.Contains("camera") ||
                name.Contains("manager") || name.Contains("canvas") || name.Contains("light") ||
                name.Contains("volume") || name.Contains("bomb") || name.Contains("event") ||
                name.Contains("portal") || name.Contains("input"))
                continue;

            // Only process platforms (Insky and Jump objects on Ground layer 6)
            bool isPlatform = t.gameObject.layer == 6 && (name.Contains("insky") || name.Contains("jump"));
            if (!isPlatform) continue;

            // Only fix root-level platforms or direct children of root platforms
            // Skip deeply nested children
            if (t.parent != null && t.parent.gameObject.layer == 6 && t.parent.parent != null && t.parent.parent.gameObject.layer == 6)
                continue;

            Vector3 pos = t.position;

            // Clamp X to center area (-1.5 to 1.5) — no edge platforms
            pos.x = Mathf.Clamp(pos.x, -1.5f, 1.5f);

            // Clamp Y to reachable range (floor is ~-2, jump reaches ~2.5 above)
            if (pos.y > 2.5f)
                pos.y = Mathf.Clamp(pos.y, -1f, 2f);
            if (pos.y < -3f)
                pos.y = -1f;

            t.position = pos;
        }

        Debug.Log("[PlatformFixer] Platforms repositioned to center.");
    }
}
