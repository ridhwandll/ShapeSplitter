using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Windows;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour, IHealth
{
    // Movement
    public float moveSpeed = 17;
    public float acceleration = 80; // Higher = snappier

    private Rigidbody2D _rigidbody2D;
    private Vector2 _dragStartPosition;
    private Vector2 _dragEndPosition;
    private bool _isAiming;
    public float shootPower = 2.0f;
    public float maxAimLineDistance = 4.0f;
    
    private TrajectorySimulator _trajectorySimulator;
    private CircleCollider2D _collider2D;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _maxSpreadAngle = 60.0f;
    [SerializeField] private Sprite _playerCore;
    [SerializeField] private Sprite _playerUnited;


    public ParticleSystem playerParticleSystem;

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

    private TrailRenderer _trailRenderer;
    private Camera _mainCamera;
    public GameObject bulletPrefab;
    public GameObject repulsorPrefab;

    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;

    private InputMaster _input;

    // Public Events available for registering (Mainly used by UIManager)
    public Action<int> OnPlayerHealthChange;
    public Action<float> OnRepulsorTimerChange;
    public Action<float> OnChainShotTimerChange;

    void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        GameObject postProcessStack = GameObject.FindGameObjectWithTag("PostProcessStack");
        postProcessStack.GetComponent<Volume>().profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        postProcessStack.GetComponent<Volume>().profile.TryGet<Vignette>(out _vignette);
        _trailRenderer = GetComponent<TrailRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        _trajectorySimulator = GetComponent<TrajectorySimulator>();

        _chromaticAberration.active = false; 
        _input = GameObject.FindGameObjectWithTag("InputMaster").GetComponent<InputManager>().Input;

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                child.gameObject.SetActive(false);
            }
        }

        ApplyShopItemLevels();
    }

    public void ApplyShopItemLevels()
    {
        _chainShotTimer = Globals.ChainShotCooldown;
        _repulsorTimer = Globals.RepulsorCooldown;

        int lifeLevel = Globals.GetShopElementLevel(ShopElementType.Life);
        Globals.PlayerMaxHealth = 50 + (lifeLevel - 1) * Globals.HealthIncreasePerLevel;

        int chainBulletLevel = Globals.GetShopElementLevel(ShopElementType.ChainBullet);
        _maxChains = 5 + (chainBulletLevel - 1); // Add one chain per level. 5 chains in level 1

        int speedLevel = Globals.GetShopElementLevel(ShopElementType.Speed);
        moveSpeed = Mathf.Lerp(12f, 20f, (speedLevel - 1) / 14f);

        int repulsorLevel = Globals.GetShopElementLevel(ShopElementType.Repulsor);
        //Globals.RepulsorCooldown = 150.0f - (repulsorLevel - 1) * Globals.RepulsorCooldownDecreasePerLevel;
        Globals.RepulsorCooldown = 20f - (repulsorLevel - 1) * Globals.RepulsorCooldownDecreasePerLevel;

        _health = Globals.PlayerMaxHealth;

        OnPlayerHealthChange?.Invoke(_health);
    }

    void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        UpdateChainShot();
        UpdateRepulsor();
        AimAndShoot();
    }
    bool IsTouchBlockedByUI()
    {
        return false;
        //return EventSystem.current.IsPointerOverGameObject();
    }

    private void SplitPlayerAndApplyForce(Vector2 biesctorDirection)
    {
        _spriteRenderer.sprite = _playerCore;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _trailRenderer.emitting = false;

        float halfAngle = _maxSpreadAngle * 0.5f;
        Vector2[] directions = { Quaternion.Euler(0, 0, halfAngle) * biesctorDirection.normalized, Quaternion.Euler(0, 0, -halfAngle) * biesctorDirection.normalized };

        int i = 0;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                child.gameObject.SetActive(true);
                child.gameObject.GetComponent<Rigidbody2D>().AddForce(directions[i].normalized * shootPower, ForceMode2D.Impulse);
                i++;
            }
        }

        //TODO: Conservation of momentum
        // Core recoil (1/4 of the original ShootPower)
        _rigidbody2D.GetComponent<Rigidbody2D>().AddForce(-biesctorDirection.normalized * (shootPower/4), ForceMode2D.Impulse);
    }

    private void UnitePlayerParts()
    {
        _spriteRenderer.sprite = _playerUnited;
        _trailRenderer.emitting = true;

        // TODO: move the splitted parts to the position of the core
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                child.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                child.gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
                child.gameObject.GetComponent<Transform>().localPosition = Vector3.zero;
                child.gameObject.GetComponent<Transform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
                child.gameObject.SetActive(false);
            }
        }
    }

    private void AimAndShoot()
    {
        if (IsTouchBlockedByUI())
            return;

        if (_input.Player.Move.WasPressedThisFrame() && _isAiming == false)
        {
            _dragStartPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
            _isAiming = true;
            StartSlowMotion();
            UnitePlayerParts();
        }

        // Draw the Aim Line
        if (_input.Player.Move.IsPressed())
        {
            Vector3 screenPos = _input.Player.PointerPosition.ReadValue<Vector2>();
            screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
            Vector2 currentTouchPos = _mainCamera.ScreenToWorldPoint(screenPos);

            Vector2 bisector = (currentTouchPos - _dragStartPosition);
            float halfAngle = _maxSpreadAngle * 0.5f;
            Vector2[] directions = { Quaternion.Euler(0, 0, halfAngle) * bisector.normalized, Quaternion.Euler(0, 0, -halfAngle) * bisector.normalized };
            _trajectorySimulator.DrawTrajectory(transform.position, directions[0], directions[1], shootPower);
        }

        if (_input.Player.Move.WasReleasedThisFrame() && _isAiming)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0.0f;
            _trajectorySimulator.ClearLinePositions();
            _dragEndPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
            Vector2 dragDirection = _dragEndPosition - _dragStartPosition; //Bisectoe

            _isAiming = false;

            SplitPlayerAndApplyForce(dragDirection);
            EndSlowMotion();
        }
    }

    private void StartSlowMotion()
    {
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    private void EndSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
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
        
        if (_health == 0) //LEVEL END HERE
        {
            Destroy(gameObject);
            GameManager.Instance.SetPlayerAlive(false);
        }
        
        if (!isDamagingByOwnBullet) 
            playerParticleSystem.Play();

        OnPlayerHealthChange?.Invoke(_health);
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(Globals.PlayerMaxHealth, _health + amount);

        if (_health > 15 && _chromaticAberration)
        {
            _chromaticAberration.active = false;
            _vignette.color.value = Color.black;
        }

        OnPlayerHealthChange?.Invoke(_health);
    }

    private void UpdateChainShot()
    {
        if (_chainShotTimer > 0f)
            _chainShotTimer -= Time.deltaTime;

        float progress = Mathf.Clamp01((Globals.ChainShotCooldown - _chainShotTimer) / Globals.ChainShotCooldown);
        OnChainShotTimerChange?.Invoke(progress * Globals.ChainShotCooldown);
        
        // Trigger ChainShot ability
        if (_input.Player.ChainShot.IsPressed() && _chainShotTimer <= 0f)        
            ActivateChainShot();        
    }
    
    private void UpdateRepulsor()
    {
        if (_repulsorTimer > 0f)
            _repulsorTimer -= Time.deltaTime;

        float progress = Mathf.Clamp01((Globals.RepulsorCooldown - _repulsorTimer) / Globals.RepulsorCooldown);
        OnRepulsorTimerChange?.Invoke(progress * Globals.RepulsorCooldown);
        
        // Trigger Repulsor ability
        if (_input.Player.Repulse.IsPressed() && _repulsorTimer <= 0f)        
        if (_input.Player.Repulse.IsPressed() && _repulsorTimer <= 0f)        
            ActivateRepulsor();        
    }
    
    public void ActivateRepulsor()
    {
        Instantiate(repulsorPrefab, transform.position, Quaternion.identity);
        TakeDamage(Globals.RepulsorSelfDamage, true);
        SoundFXManager.instance.PlaySoundFXClip(repulsorSound, 0.8f);
        _repulsorTimer = Globals.RepulsorCooldown;
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

    public void ActivateChainShot()
    {
        Transform first = FindNextTarget(transform.position, new HashSet<Transform>());
        if (first != null)
        {
            StartCoroutine(ChainShotRoutine(first));
            TakeDamage(Globals.ChainShotSelfDamage, true);
            _chainShotTimer = Globals.ChainShotCooldown;
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
