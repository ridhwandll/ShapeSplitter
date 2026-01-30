using System;
using System.Collections;
using System.Collections.Generic;
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
    private  CircleCollider2D _circleCollider2D;
    private float _initialColliderRadius;
        
    public Vector2 minBounds;
    public Vector2 maxBounds;
    
    // Health
    private int _health;
    
    // ChainShot
    private float _chainShotTimer = 0f;
    private int _maxChains = 6;
    private float _chainRange = 15f;
    public float _chainDelay = 0.2f;
    public float chainLineLifetime = 0.4f;
    public LayerMask enemyMask;
    public AudioClip chainShotSound;
    public LineRenderer chainLinePrefab;
    
    //Repulsor
    private float _repulsorTimer = 0f;
    public AudioClip repulsorSound;
    
    private Vector3 _currentVelocity;

    private LineRenderer _lineRenderer;
    private TrailRenderer _trailRenderer;
    private Camera _mainCamera;
    public GameObject bulletPrefab;
    public GameObject repulsorPrefab;
    private float _nextFireTime;
    
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _lineRenderer = GetComponent<LineRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _trailRenderer.emitting = false;
        ApplyShopItemLevels();
    }

    void Start()
    {
        GameObject.FindGameObjectWithTag("PostProcessStack").GetComponent<Volume>().profile.TryGet<ChromaticAberration>(out var b);
        GameObject.FindGameObjectWithTag("PostProcessStack").GetComponent<Volume>().profile.TryGet<Vignette>(out var v);
        _chromaticAberration = b;
        _vignette = v;
        _chromaticAberration.active = false;
        _chainShotTimer = Globals.ChainShotCooldown;
        _repulsorTimer = Globals.RepulsorCooldown;
        _initialColliderRadius = _circleCollider2D.radius;
    }

    public void ApplyShopItemLevels()
    {
        // Dash buffs
        int dashLevel = Globals.GetShopElementLevel(ShopElementType.Dash);
        Globals.PlayerDashCooldown = 12f - (dashLevel - 1) * Globals.DashCooldownDecreasePerLevel;
        
        float dashSpeedIncreasePerLevel = (30f - 20f) / 14f;
        dashSpeed = 20f + (dashLevel - 1) * dashSpeedIncreasePerLevel;
        
        int lifeLevel = Globals.GetShopElementLevel(ShopElementType.Life);
        Globals.PlayerMaxHealth = 50 + (lifeLevel - 1) * Globals.HealthIncreasePerLevel;
        
        int chainBulletLevel = Globals.GetShopElementLevel(ShopElementType.ChainBullet);
        _maxChains = 5 + (chainBulletLevel - 1); // Add one chain per level. 5 chains in level 1
        
        int speedLevel = Globals.GetShopElementLevel(ShopElementType.Speed);
        moveSpeed = Mathf.Lerp(12f, 20f, (speedLevel - 1) / 14f);
        
        int repulsorLevel = Globals.GetShopElementLevel(ShopElementType.Repulsor);
        Globals.RepulsorCooldown = 150.0f - (repulsorLevel - 1) * Globals.RepulsorCooldownDecreasePerLevel;
        
        _health = Globals.PlayerMaxHealth;
        gameScreenUIManager.UpdatePlayerHealth(_health);
        
        Debug.Log("PlayerDashCooldown: " + Globals.PlayerDashCooldown);
        Debug.Log("DashSpeed: " + dashSpeed);
        Debug.Log("MoveSpeed: " + moveSpeed);
        Debug.Log("PlayerMaxHealth: " + Globals.PlayerMaxHealth);
        Debug.Log("MaxChains: " + _maxChains);
        Debug.Log("Repulsor cooldown: " + Globals.RepulsorCooldown);
    }
    
    void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        //DASH
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
                EndDash();
        }
        
        UpdateChainShot();
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
        SoundFXManager.instance.PlaySoundFXClip(dashSound, 0.5f);
        playerParticleSystem.Play();
        
        _circleCollider2D.radius = _initialColliderRadius + 0.2f;
        
        TakeDamage(Globals.DashSelfDamage, true);
    }

    private void EndDash()
    {
        _trailRenderer.emitting = false;
        _isDashing = false;
        _lastDashTime = Time.time;
        _circleCollider2D.radius = _initialColliderRadius;
    }
    
    private void MovePlayer()
    {
        ////// Translation Based Movement //////
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, vertical, 0f).normalized;

        _currentVelocity = Vector3.Lerp(_currentVelocity, inputDir * moveSpeed, acceleration * Time.deltaTime);
        
        transform.position += _currentVelocity * Time.deltaTime;
        
        //clamp the movement
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
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

        if (_health <= 15 && _chromaticAberration)
        {
            _chromaticAberration.active = true;
            _vignette.color.value = Color.red;
        }
        
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
        {
            _chromaticAberration.active = false;
            _vignette.color.value = Color.black;
        }
        
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

    private void UpdateChainShot()
    {
        if (_chainShotTimer > 0f)
            _chainShotTimer -= Time.deltaTime;

        float progress = Mathf.Clamp01((Globals.ChainShotCooldown - _chainShotTimer) / Globals.ChainShotCooldown);
        gameScreenUIManager.UpdateChainShotSlider(progress * Globals.ChainShotCooldown);
        
        // Trigger ChainShot ability
        if (Input.GetKeyDown(KeyCode.E) && _chainShotTimer <= 0f)
        {
            ActivateChainShot();
            _chainShotTimer = Globals.ChainShotCooldown;
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
    
    private void ActivateRepulsor()
    {
        Instantiate(repulsorPrefab, transform.position, Quaternion.identity);
        TakeDamage(Globals.RepulsorSelfDamage, true);
        SoundFXManager.instance.PlaySoundFXClip(repulsorSound, 0.8f);
        _mainCamera.gameObject.GetComponent<CameraShake>().Shake(0.3f, 8, 0.4f);
    }
    
    // ChainShot
    Transform FindNextTarget(Vector2 from, HashSet<Transform> hit)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(from, _chainRange, enemyMask);

        float closest = float.MaxValue;
        Transform best = null;

        foreach (var c in hits)
        {
            Transform t = c.transform;
            if (hit.Contains(t))
                continue;

            IHealth h = t.GetComponent<IHealth>();
            if (h == null)
                continue;

            float d = Vector2.Distance(from, t.position);
            if (d < closest)
            {
                closest = d;
                best = t;
            }
        }
        return best;
    }
    
    void ActivateChainShot()
    {
        Transform first = FindNextTarget(transform.position, new HashSet<Transform>());
        if (first != null)
        {
            StartCoroutine(ChainShotRoutine(first));
            TakeDamage(Globals.ChainShotSelfDamage, true);
        }
    }
    
    IEnumerator ChainLineFadeOut(LineRenderer lineRenderer)
    {
        float timeElapsed = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;

        while (timeElapsed < chainLineLifetime)
        {
            timeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timeElapsed / chainLineLifetime);

            // Update alpha for both start and end colors
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

            yield return null;
        }

        // Ensure it is completely invisible
        lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, 0f);

        Destroy(lineRenderer.gameObject);
    }
    void SpawnChainLine(Vector3 from, Vector3 to)
    {
        LineRenderer lr = Instantiate(chainLinePrefab);
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startColor = Color.orange;
        lr.endColor = Color.orangeRed;
        lr.startWidth = 0.15f;
        lr.endWidth = 0.05f;
        lr.numCornerVertices = 5;
        lr.numCapVertices = 5;
        
        SoundFXManager.instance.PlaySoundFXClip(chainShotSound, 0.6f);
        _mainCamera.gameObject.GetComponent<CameraShake>().Shake(0.4f, 3, 0.1f);
        
        StartCoroutine(ChainLineFadeOut(lr));
    }
    
    IEnumerator ChainShotRoutine(Transform firstTarget)
    {
        HashSet<Transform> hitEnemies = new HashSet<Transform>();
        Transform currentTarget = firstTarget;
        Vector3 lastTarget = transform.position; //Last target is player, initial line start

        for (int i = 0; i < _maxChains; i++)
        {
            if (currentTarget == null)
                yield break;

            hitEnemies.Add(currentTarget);
            Vector3 curerntPos = currentTarget.position; // Store it locally as we are destroying Enemy gameObject later
            
            SpawnChainLine(lastTarget, curerntPos);
            
            currentTarget.GetComponent<IHealth>()?.TakeDamage(Globals.ChainShotDamage, false);
            
            yield return new WaitForSeconds(_chainDelay);
            
            lastTarget = curerntPos;
            currentTarget = FindNextTarget(curerntPos, hitEnemies);
        }
    }
}
