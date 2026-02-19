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

    private LineRenderer _lineRenderer1;
    private LineRenderer _lineRenderer2;

    private List<Vector3> trajectoryPoints = new List<Vector3>();

    void Awake()
    {
        _playerRigidBody = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<Collider2D>();

        _lineRenderer1 = Instantiate(trajectoryRendererObj, transform).GetComponent<LineRenderer>();
        _lineRenderer2 = Instantiate(trajectoryRendererObj, transform).GetComponent<LineRenderer>();

        CopyLineRendererSettings(_lineRenderer1, _lineRenderer2);
        CreateGhostScene();
    }

    void Destroy()
    {
        Destroy(_lineRenderer1.gameObject);
        Destroy(_lineRenderer2.gameObject);
    }

    private void CopyLineRendererSettings(LineRenderer source, LineRenderer target)
    {
        target.material = source.material;
        target.startWidth = source.startWidth;
        target.endWidth = source.endWidth;
        target.widthMultiplier = source.widthMultiplier;
        target.widthCurve = source.widthCurve;

        target.startColor = source.startColor;
        target.endColor = source.endColor;
        target.colorGradient = source.colorGradient;

        target.alignment = source.alignment;
        target.textureMode = source.textureMode;
        target.numCapVertices = source.numCapVertices;
        target.numCornerVertices = source.numCornerVertices;

        target.shadowCastingMode = source.shadowCastingMode;
        target.receiveShadows = source.receiveShadows;

        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;

        target.loop = source.loop;
        target.useWorldSpace = source.useWorldSpace;
    }

    private void CreateGhostScene()
    {
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

    private void CloneWalls()
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

    private void CopyCollider(Collider2D original, GameObject target)
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

    public void ClearLinePositions()
    {
        _lineRenderer1.positionCount = 0;
        _lineRenderer2.positionCount = 0;
    }

    // Call every frame while aiming
    public void DrawTrajectory(Vector2 startPosition, Vector2 dir1, Vector2 dir2, float shootPower)
    {
        SimulateSingleTrajectory(startPosition, dir1, shootPower, _lineRenderer1);
        SimulateSingleTrajectory(startPosition, dir2, shootPower, _lineRenderer2);
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
