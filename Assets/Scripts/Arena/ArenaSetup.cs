using UnityEngine;

/// <summary>
/// Generates the arena at runtime: floor + boundary walls.
/// Attach this to an empty GameObject in the scene.
/// </summary>
public class ArenaSetup : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private float _arenaSize = 40f;
    [SerializeField] private float _wallHeight = 3f;
    [SerializeField] private float _wallThickness = 1f;

    [Header("Materials (assign in Inspector)")]
    [SerializeField] private Material _floorMaterial;
    [SerializeField] private Material _wallMaterial;

    void Awake()
    {
        CreateFloor();
        CreateWalls();
    }

    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Arena Floor";
        floor.transform.parent = transform;
        floor.transform.localPosition = Vector3.zero;
        // Rotate plane to face the camera (XY plane background)
        floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        floor.transform.localScale = new Vector3(_arenaSize / 10f, 1f, _arenaSize / 10f);

        if (_floorMaterial != null)
            floor.GetComponent<Renderer>().material = _floorMaterial;
    }

    private void CreateWalls()
    {
        float halfSize = _arenaSize / 2f;

        // Top wall (positive Y)
        CreateWall("Wall_Top", new Vector3(0, halfSize, 0),
            new Vector3(_arenaSize + _wallThickness, _wallThickness, _wallHeight));
        // Bottom wall (negative Y)
        CreateWall("Wall_Bottom", new Vector3(0, -halfSize, 0),
            new Vector3(_arenaSize + _wallThickness, _wallThickness, _wallHeight));
        // Right wall (positive X)
        CreateWall("Wall_Right", new Vector3(halfSize, 0, 0),
            new Vector3(_wallThickness, _arenaSize + _wallThickness, _wallHeight));
        // Left wall (negative X)
        CreateWall("Wall_Left", new Vector3(-halfSize, 0, 0),
            new Vector3(_wallThickness, _arenaSize + _wallThickness, _wallHeight));
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (!string.IsNullOrEmpty(name))
            wall.name = name;
        else
            wall.name = "ArenaWall";
        wall.tag = "Wall";
        wall.transform.parent = transform;
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;

        if (_wallMaterial != null)
            wall.GetComponent<Renderer>().material = _wallMaterial;
    }
}
