using UnityEngine;

public class PlayerController : MonoBehaviour, IHealth
{
    public float moveSpeed = 35;
    public float counterMovement = 13;
    public float maxAimLineDistance = 5.0f;
    public float fireRate = 0.2f;
    
    // Health
    private int _health;
    private int _maxHealth = 50;
    
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
        AimAndShoot();
    }

    private void FixedUpdate()
    {
        float velocity =  _rigidBody2D.linearVelocity.magnitude;
        float maxSpeed =  moveSpeed * 1.5f;

        if (_input.x > 0 && _rigidBody2D.linearVelocity.x > maxSpeed) _input.x = 0;
        if (_input.x > 0 && _rigidBody2D.linearVelocity.x < -maxSpeed)  _input.x = 0;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y > maxSpeed)  _input.y = 0;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < -maxSpeed) _input.y = 0;

        float xBonus = 1.0f;
        float yBonus = 1.0f;
        
        if (_input.x < 0 && _rigidBody2D.linearVelocity.x > 0) xBonus = 2.0f;
        if (_input.x > 0 && _rigidBody2D.linearVelocity.x < 0)  xBonus = 2.0f;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < 0)  yBonus = 2.0f;
        if (_input.y > 0 && _rigidBody2D.linearVelocity.y < 0) yBonus = 2.0f;
        
        float extraForce = maxSpeed - velocity;
        
        _rigidBody2D.AddForce(_input * moveSpeed * extraForce * xBonus * yBonus * 2 * Time.fixedDeltaTime);
        _rigidBody2D.AddForce(counterMovement * -_rigidBody2D.linearVelocity * moveSpeed * Time.fixedDeltaTime);
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
