using UnityEngine;

public class PlayerController : MonoBehaviour, IHealth
{
    public float moveSpeed = 35;
    public float acceleration = 10f; // higher = snappier

    public float maxAimLineDistance = 5.0f;
    public float fireRate = 0.2f;
    
    // Health
    private int _health;
    private int _maxHealth = 50;
    
    private Vector3 _currentVelocity;
    private Vector2 _input;
    private Rigidbody2D _rigidBody2D;
    private LineRenderer _lineRenderer;
    private Camera _mainCamera;
    public GameObject bulletPrefab;
    private float _nextFireTime;
    private UIManager _uiManager;
    
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }

    void Start()
    {
        _health = _maxHealth;
        _uiManager.UpdatePlayerHealth(_health);
    }
    
    void Update()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MovePlayer();
        AimAndShoot();
    }
    
    private void MovePlayer()
    {
        ////// Translation Based Movement //////
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, vertical, 0f).normalized;

        _currentVelocity = Vector3.Lerp(_currentVelocity, inputDir * moveSpeed, acceleration * Time.deltaTime);
        
        transform.position += _currentVelocity * Time.deltaTime;
    }
    
    private void AimAndShoot()
    {
        // Draw Aim Line
        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 start = transform.position;
        Vector2 end = (mousePos - transform.position);
 
        if (end.magnitude > maxAimLineDistance)
            end = end.normalized * maxAimLineDistance;
        
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, start + end);
        
        // SHOOT
        Vector3 aimDir = mousePos - transform.position;
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            var bullerSpawnPos = transform.position + (aimDir.normalized * 0.5f);
            GameObject bullet = Instantiate(bulletPrefab, bullerSpawnPos, Quaternion.identity);
            bullet.GetComponent<Bullet>().Setup(aimDir);
            _nextFireTime = Time.time + fireRate;
            
            TakeDamage(1); // Shooting 1 bullet does 1 damage to self
        }
    }
    
    public int GetCurrentHealth() => _health;
    public int GetMaxHealth() => _maxHealth;

    public void TakeDamage(int amount)
    {
        _health = Mathf.Max(0, _health - amount);
        if (_health == 0) //TODO: LEVEL END HERE
        {
            Destroy(gameObject);
            Constants.IsPlayerAlive = false;
        }
        
        _uiManager.UpdatePlayerHealth(_health);
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
        _uiManager.UpdatePlayerHealth(_health);
    }
}
