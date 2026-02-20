using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TrajectorySimulator : MonoBehaviour
{
    [Header("Simulation Settings")]
    public LayerMask wallsLayerMask;
    public int maxSimulationSteps = 200;
    public float timeStep = 0.02f;

    [SerializeField] private GameObject trajectoryRendererObj;

    // --- Internal ghost objects ---
    private PhysicsScene2D ghostPhysicsScene;
    private Scene ghostScene;
    private GameObject ghostObject;
    private Rigidbody2D ghostRb;

    // Player References
    private Rigidbody2D _playerRigidBody;
    private Collider2D _playerCollider;

    private LineRenderer[] _lineRenderers;

    private List<Vector3> trajectoryPoints = new List<Vector3>();

    void Start()
    {
        _playerRigidBody = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<Collider2D>();

        
    }

    public void Initialize(int splitShapeCount)
    {
        _lineRenderers = new LineRenderer[splitShapeCount];
        for (int i = 0; i < splitShapeCount; i++)
        {
            _lineRenderers[i] = Instantiate(trajectoryRendererObj, transform).GetComponent<LineRenderer>();
        }
        CreateGhostScene();
    }

    void Destroy()
    {
        foreach(LineRenderer lineRenderer in _lineRenderers)
            Destroy(lineRenderer.gameObject);
    }

    private void CreateGhostScene()
    {
        void CopyCollider(Collider2D original, GameObject target)
        {
            if (original is CircleCollider2D circle)
            {
                var c = target.AddComponent<CircleCollider2D>();
                c.radius = circle.radius;
                c.offset = circle.offset;
                c.sharedMaterial = circle.sharedMaterial;
            }
            else if (original is PolygonCollider2D poly)
            {
                var p = target.AddComponent<PolygonCollider2D>();
                p.points = poly.points;
                p.offset = poly.offset;
                p.sharedMaterial = poly.sharedMaterial;
            }
            else if (original is BoxCollider2D box)
            {
                var b = target.AddComponent<BoxCollider2D>();
                b.size = box.size;
                b.offset = box.offset;
                b.sharedMaterial = box.sharedMaterial;
            }
            else if (original is EdgeCollider2D edge)
            {
                var e = target.AddComponent<EdgeCollider2D>();
                e.points = edge.points;
                e.sharedMaterial = edge.sharedMaterial;
            }
        }

        void CloneWalls()
        {
            // Find all GameObjects in scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                // Check if object is in wallsLayerMask
                if (((wallsLayerMask.value >> go.layer) & 1) == 0)
                    continue;

                Collider2D col = go.GetComponent<Collider2D>();
                if (col == null)
                    continue;

                // Create ghost object in ghost scene
                GameObject wallGhost = new GameObject("Ghost_" + go.name);
                SceneManager.MoveGameObjectToScene(wallGhost, ghostScene);

                // Copy collider
                CopyCollider(col, wallGhost);

                // Copy transform
                wallGhost.transform.position = go.transform.position;
                wallGhost.transform.rotation = go.transform.rotation;
                wallGhost.transform.localScale = go.transform.lossyScale;

                // Hide renderers
                foreach (var r in wallGhost.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }
        }

        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        ghostScene = SceneManager.CreateScene("TrajectoryGhostScene", parameters);
        ghostPhysicsScene = ghostScene.GetPhysicsScene2D();

        ghostObject = new GameObject("TrajectoryGhostPlayer");
        SceneManager.MoveGameObjectToScene(ghostObject, ghostScene);

        ghostRb = ghostObject.AddComponent<Rigidbody2D>();
        ghostRb.gravityScale = _playerRigidBody.gravityScale;
        ghostRb.interpolation = _playerRigidBody.interpolation;
        ghostRb.collisionDetectionMode = _playerRigidBody.collisionDetectionMode;
        ghostRb.linearDamping = _playerRigidBody.linearDamping;
        ghostRb.angularDamping = _playerRigidBody.angularDamping;
        ghostRb.freezeRotation = _playerRigidBody.freezeRotation;

        CopyCollider(_playerCollider, ghostObject);
        ghostObject.SetActive(false);

        CloneWalls(); // Clone the walls and the collider in it
    }

    public void ClearLinePositions()
    {
        foreach (LineRenderer lr in _lineRenderers)        
            lr.positionCount = 0;
        
    }

    // Call every frame while aiming
    public void DrawTrajectory(Vector2 startPosition, List<Vector2> directions, float shootPower)
    {
        for (int i = 0; i < directions.Count; i++)        
            SimulateSingleTrajectory(startPosition, directions[i], shootPower, _lineRenderers[i]);
    }

    private void SimulateSingleTrajectory(Vector2 startPosition, Vector2 direction, float shootPower, LineRenderer lr)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        ghostObject.SetActive(true);
        ghostObject.transform.position = startPosition;
        ghostObject.transform.rotation = Quaternion.identity;
        ghostRb.linearVelocity = direction.normalized * (shootPower / _playerRigidBody.mass);
        ghostRb.angularVelocity = 0;

        trajectoryPoints.Clear();
        trajectoryPoints.Add(startPosition);

        for (int i = 0; i < maxSimulationSteps; i++)
        {
            ghostPhysicsScene.Simulate(timeStep);
            trajectoryPoints.Add(ghostRb.position);
            if (ghostRb.linearVelocity.magnitude < 0.01f) break;
        }

        lr.positionCount = trajectoryPoints.Count;
        lr.SetPositions(trajectoryPoints.ToArray());

        ghostObject.SetActive(false);
    }
}
