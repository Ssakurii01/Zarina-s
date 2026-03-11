using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    [Header("Arena Settings (Portrait 9:16)")]
    [SerializeField] private float _arenaWidth = 18f;
    [SerializeField] private float _arenaHeight = 32f;
    [SerializeField] private float _wallHeight = 3f;
    [SerializeField] private float _wallThickness = 1f;

    [Header("Materials (assign in Inspector)")]
    [SerializeField] private Material _floorMaterial;
    [SerializeField] private Material _wallMaterial;

    public float ArenaWidth => _arenaWidth;
    public float ArenaHeight => _arenaHeight;

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
        floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        floor.transform.localScale = new Vector3(_arenaWidth / 10f, 1f, _arenaHeight / 10f);

        if (_floorMaterial != null)
            floor.GetComponent<Renderer>().material = _floorMaterial;
    }

    private void CreateWalls()
    {
        float halfW = _arenaWidth / 2f;
        float halfH = _arenaHeight / 2f;

        CreateWall("Wall_Top", new Vector3(0, halfH, 0),
            new Vector3(_arenaWidth + _wallThickness, _wallThickness, _wallHeight));
        CreateWall("Wall_Bottom", new Vector3(0, -halfH, 0),
            new Vector3(_arenaWidth + _wallThickness, _wallThickness, _wallHeight), isGround: true);
        CreateWall("Wall_Right", new Vector3(halfW, 0, 0),
            new Vector3(_wallThickness, _arenaHeight + _wallThickness, _wallHeight));
        CreateWall("Wall_Left", new Vector3(-halfW, 0, 0),
            new Vector3(_wallThickness, _arenaHeight + _wallThickness, _wallHeight));
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale, bool isGround = false)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.tag = "Wall";
        wall.transform.parent = transform;
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;

        if (isGround)
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) wall.layer = groundLayer;
        }

        if (_wallMaterial != null)
            wall.GetComponent<Renderer>().material = _wallMaterial;
    }
}
