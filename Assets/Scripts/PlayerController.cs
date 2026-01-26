using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour, IHealth
{
    // Movement
    public float moveSpeed = 17;
    public float acceleration = 80; // Higher = snappier
    
    public float maxAimLineDistance = 4.0f;
    public float fireRate = 0.3f;
    
    public float dashSpeed = 20;
    public float dashDuration = 0.2f;
    public UIManager gameScreenUIManager;
    
    public ParticleSystem playerParticleSystem;
    public AudioClip dashSound;
    
    private bool _isDashing;
    private float _dashTimeLeft;
    private float _lastDashTime;
    private Vector3 _dashDirection;
    
    // Health
    private int _health;
    
    // Health Regen
    private int _regenAmount = 5;
    private float _regenInterval = 1.0f;
    private float _regenTimer = 0f;
    
    //Repulsor
    private float _repulsorTimer = 0f;
    public AudioClip repulsorSound;
    
    private Vector3 _currentVelocity;
    private Vector2 _input;
    private Rigidbody2D _rigidBody2D;
    private LineRenderer _lineRenderer;
    private TrailRenderer _trailRenderer;
    private Camera _mainCamera;
    public GameObject bulletPrefab;
    public GameObject repulsorPrefab;
    private float _nextFireTime;
    
    private ChromaticAberration _chromaticAberration;
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRenderer.emitting = false;
        ApplyShopItemLevels();
    }

    void Start()
    {
        GameObject.FindGameObjectWithTag("PostProcessStack").GetComponent<Volume>().profile .TryGet<ChromaticAberration>(out var b);
        _chromaticAberration = b;
        _chromaticAberration.active = false;
        _repulsorTimer = Globals.RepulsorCooldown;
        
    }

    public void ApplyShopItemLevels()
    {
        // Dash buffs
        int dashLevel = Globals.GetShopElementLevel(ShopElementType.Dash);
        Globals.PlayerDashCooldown = 12f - (dashLevel - 1) * Globals.DashCooldownDecreasePerLevel;
        
        float dashSpeedIncreasePerLevel = (30f - 20f) / 14f;
        dashSpeed = 20f + (dashLevel - 1) * dashSpeedIncreasePerLevel;
        
        int lifeLevel = Globals.GetShopElementLevel(ShopElementType.Life);
        Globals.PlayerMaxHealth = Mathf.RoundToInt(50f + (lifeLevel - 1) * Globals.HealthIncreasePerLevel);
        
        int lifeRegen = Globals.GetShopElementLevel(ShopElementType.LifeRegen);
        _regenInterval = Mathf.Lerp(30f, 10f, (lifeRegen - 1f) / 14f); // 30f sec regen at level 1, 10f s at level 15
        
        int speedLevel = Globals.GetShopElementLevel(ShopElementType.Speed);
        moveSpeed = Mathf.Lerp(12f, 22f, (speedLevel - 1f) / 14f);
        
        int repulsorLevel = Globals.GetShopElementLevel(ShopElementType.Repulsor);
        Globals.RepulsorCooldown = 65.0f - (repulsorLevel - 1) * Globals.RepulsorCooldownDecreasePerLevel;
        
        _health = Globals.PlayerMaxHealth;
        gameScreenUIManager.UpdatePlayerHealth(_health);
        
        Debug.Log("PlayerDashCooldown: " + Globals.PlayerDashCooldown);
        Debug.Log("DashSpeed: " + dashSpeed);
        Debug.Log("MoveSpeed: " + moveSpeed);
        Debug.Log("PlayerMaxHealth: " + Globals.PlayerMaxHealth);
        Debug.Log("Life Regen Interval: " + _regenInterval);
    }
    
    void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        //DASH
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (!_isDashing && Input.GetKeyDown(KeyCode.Space) && Time.time >= _lastDashTime + Globals.PlayerDashCooldown)
            TryStartDash();

        if (!_isDashing)
        {
            float elapsedDash = Time.time - _lastDashTime;
            gameScreenUIManager.UpdateDashSlider(Mathf.Clamp(elapsedDash, 0f, Globals.PlayerDashCooldown));
        }

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
        
        HandleLifeRegen();
        UpdateRepulsor();
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
        _mainCamera.gameObject.GetComponent<CameraShake>().Shake(0.4f, 3, 0.01f);
        SoundFXManager.instance.PlaySoundFXClip(dashSound, transform, 0.5f);
        playerParticleSystem.Play();
        
        TakeDamage(Globals.OwnBulletDamage * 3, true); //Dashing does 4 damage to self
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
            
            TakeDamage(Globals.OwnBulletDamage, true); // Shooting 1 bullet does 1 damage to self
        }
    }
    
    public int GetCurrentHealth() => _health;
    public int GetMaxHealth() => Globals.PlayerMaxHealth;

    public void TakeDamage(int amount, bool isDamagingByOwnBullet = false)
    {
        _health = Mathf.Max(0, _health - amount);
        
        if (_health <= 15&& _chromaticAberration)
            _chromaticAberration.active = true;
        
        if (_health == 0) //TODO: LEVEL END HERE
        {
            Destroy(gameObject);
            GameManager.Instance.SetPlayerAlive(false);
        }
        
        if (!isDamagingByOwnBullet)               
            playerParticleSystem.Play();
        
        gameScreenUIManager.UpdatePlayerHealth(_health);
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(Globals.PlayerMaxHealth, _health + amount);

        if (_health > 15 && _chromaticAberration)
            _chromaticAberration.active = false;
        
        gameScreenUIManager.UpdatePlayerHealth(_health);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && _isDashing)
        {
            Debug.Log("Enemy Hit");
            IHealth health = other.GetComponent<IHealth>();
            health.TakeDamage(Globals.DashDamage);            
        }
    }

    void HandleLifeRegen()
    {
        _regenTimer += Time.deltaTime;

        if (_regenTimer >= _regenInterval)
        {
            _regenTimer -= _regenInterval;
            Heal(_regenAmount);
        }
    }

    private void UpdateRepulsor()
    {
        if (_repulsorTimer > 0f)
            _repulsorTimer -= Time.deltaTime;

        float progress = Mathf.Clamp01((Globals.RepulsorCooldown - _repulsorTimer) / Globals.RepulsorCooldown);
        gameScreenUIManager.UpdateRepulsorSlider(progress * Globals.RepulsorCooldown);
        
        // Trigger Repulsor ability
        if (Input.GetKeyDown(KeyCode.X) && _repulsorTimer <= 0f)
        {
            ActivateRepulsor();
            _repulsorTimer = Globals.RepulsorCooldown;
        }
    }
    
    public float GetRepulsorTimer() => _repulsorTimer;
    
    
    private void ActivateRepulsor()
    {
        Instantiate(repulsorPrefab, transform.position, Quaternion.identity);
        TakeDamage(Globals.OwnBulletDamage * 8, true); //Repulsing does 8 damage to self
        SoundFXManager.instance.PlaySoundFXClip(repulsorSound, transform, 0.8f);
        _mainCamera.gameObject.GetComponent<CameraShake>().Shake(0.3f, 8, 0.4f);
    }
}
