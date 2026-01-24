using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHealth
{
    // Movement
    public float moveSpeed = 35;
    public float acceleration = 10f; // Higher = snappier
    
    public float maxAimLineDistance = 5.0f;
    public float fireRate = 0.2f;
    
    public float dashSpeed = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    
    public ParticleSystem playerParticleSystem;
    
    private bool _isDashing;
    private float _dashTimeLeft;
    private float _lastDashTime;
    private Vector3 _dashDirection;
    
    // Health
    private int _health;
    private int _maxHealth = 50;
    
    private Vector3 _currentVelocity;
    private Vector2 _input;
    private Rigidbody2D _rigidBody2D;
    private LineRenderer _lineRenderer;
    private TrailRenderer _trailRenderer;
    private Camera _mainCamera;
    public GameObject bulletPrefab;
    private float _nextFireTime;
    private UIManager _uiManager;
    
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        _trailRenderer.emitting = false;
    }

    void Start()
    {
        _health = _maxHealth;
        _uiManager.UpdatePlayerHealth(_health);
    }
    
    void Update()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (!_isDashing && Input.GetKeyDown(KeyCode.Space) && Time.time >= _lastDashTime + dashCooldown)
            TryStartDash();

        if (_isDashing)
        {
            transform.position += _dashDirection * dashSpeed * Time.deltaTime;
            _dashTimeLeft -= Time.deltaTime;

            if (_dashTimeLeft <= 0f)
            {
                _trailRenderer.emitting = false;
                _isDashing = false;
                _lastDashTime = Time.time;
            }
        }
        MovePlayer();
        AimAndShoot();
    }

    private void TryStartDash()
    {
        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 aimDir = mousePos - transform.position;
        
        _trailRenderer.Clear();
        _trailRenderer.emitting = true;
        _trailRenderer.time = dashDuration;
        
        _dashDirection = (aimDir).normalized;
        _dashTimeLeft = dashDuration;
        _isDashing = true;
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
            
            TakeDamage(Constants.OwnBulletDamage, true); // Shooting 1 bullet does 1 damage to self
        }
    }
    
    public int GetCurrentHealth() => _health;
    public int GetMaxHealth() => _maxHealth;

    public void TakeDamage(int amount, bool isDamagingByOwnBullet = false)
    {
        _health = Mathf.Max(0, _health - amount);
        if (_health == 0) //TODO: LEVEL END HERE
        {
            Destroy(gameObject);
            Constants.IsPlayerAlive = false;
        }
        
        if (!isDamagingByOwnBullet)               
            playerParticleSystem.Play();
        
        _uiManager.UpdatePlayerHealth(_health);
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
        _uiManager.UpdatePlayerHealth(_health);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && _isDashing)
        {
            Debug.Log("Enemy Hit");
            IHealth health = other.GetComponent<IHealth>();
            health.TakeDamage(Constants.DashDamage);            
        }
    }
}
