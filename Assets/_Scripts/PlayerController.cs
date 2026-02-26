using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum PlayerState
{
    UNITED,
    SPLIT
}

public class PlayerController : MonoBehaviour, IHealth
{
    [SerializeField] private int _splitShapeCount = 2;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _dragStartPosition;
    private Vector2 _dragEndPosition;
    private bool _isAiming;
    public float shootPower = 2.0f;
    
    [SerializeField] GameObject _playerSplitPrefab;
    private Rigidbody2D[] _splitObjectsRigidbody2D;
    private bool[] _isSplitShapeUnited = new bool[2];

    PlayerState _playerState;

    private TrajectorySimulator _trajectorySimulator;
    private CircleCollider2D _collider2D;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _maxDragDistance = 10.0f;
    [SerializeField] private float _maxSpreadAngle = 60.0f;
    [SerializeField] private float _minSpreadAngle = 15.0f;
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
        _playerState = PlayerState.UNITED;

        _splitObjectsRigidbody2D = new Rigidbody2D[_splitShapeCount];
        _isSplitShapeUnited = new bool[_splitShapeCount];

        for (int j = 0; j < _splitShapeCount; j++)
        {
            _isSplitShapeUnited[j] = false;
            _splitObjectsRigidbody2D[j] = Instantiate(_playerSplitPrefab, transform).GetComponent<Rigidbody2D>();
            _splitObjectsRigidbody2D[j].gameObject.SetActive(false);
        }

        _trajectorySimulator.Initialize(_splitShapeCount);
        ApplyShopItemLevels();
    }

    public void ApplyShopItemLevels()
    {
        /*_chainShotTimer = Globals.ChainShotCooldown;
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

        OnPlayerHealthChange?.Invoke(_health);*/
    }

    void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        UpdateChainShot();
        UpdateRepulsor();
        AimAndShoot();
    }

    private void SplitPlayerAndApplyForce(Vector2 biesctorDirection, Vector2 dragStartPoint, Vector2 dragEndPoint)
    {
        _playerState = PlayerState.SPLIT;
        _spriteRenderer.sprite = _playerCore;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _trailRenderer.emitting = false;

        var directions = GetSplitShapeDirections(biesctorDirection, dragEndPoint);

        int i = 0;
        foreach (Rigidbody2D splitObjectRb in _splitObjectsRigidbody2D)
        {
            splitObjectRb.gameObject.SetActive(true);
            splitObjectRb.AddForce(directions[i].normalized * shootPower, ForceMode2D.Impulse);
            i++;
        }

        //TODO: Conservation of momentum
        _rigidbody2D.GetComponent<Rigidbody2D>().AddForce(-biesctorDirection.normalized * (shootPower/4), ForceMode2D.Impulse);
    }

    
    private void UnitePlayerParts()
    {
        for (int i = 0; i < _splitShapeCount; i++)
        {
            Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];

            splitObjectRb.linearVelocity = (_rigidbody2D.position - (Vector2)splitObjectRb.transform.position).normalized * shootPower;
            splitObjectRb.angularVelocity = 0.0f;

            splitObjectRb.transform.localPosition = Vector3.Lerp(splitObjectRb.transform.localPosition, Vector3.zero, Time.unscaledDeltaTime * 7);
            splitObjectRb.transform.localRotation = Quaternion.Lerp(splitObjectRb.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.unscaledDeltaTime * 7);

            if (splitObjectRb.transform.localPosition.magnitude < 1.5f)
            {
                if (splitObjectRb.gameObject.activeInHierarchy)
                {
                    _isSplitShapeUnited[i] = true;
                    splitObjectRb.transform.localPosition = Vector3.zero;
                    splitObjectRb.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Debug.Log("United split object number: " + i);
                    playerParticleSystem.Play();
                    splitObjectRb.gameObject.SetActive(false);
                }
            }
        }

        if (_isSplitShapeUnited.All(n => n == true))
        {
            _playerState = PlayerState.UNITED;
            _spriteRenderer.sprite = _playerUnited;
            _trailRenderer.emitting = true;

            for (int j = 0; j < _isSplitShapeUnited.Length; j++)
                _isSplitShapeUnited[j] = false;
        }
    }

    private void AimAndShoot()
    {
        void SetSplitShapesColliderToTrigger(bool trigger)
        {
            for (int i = 0; i < _splitShapeCount; i++)
            {
                Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];
                if (splitObjectRb.gameObject.activeInHierarchy)
                {
                    splitObjectRb.gameObject.GetComponent<CircleCollider2D>().isTrigger = trigger;
                }
            }
        }

        if (_playerState == PlayerState.UNITED)
        {
            if (_input.Player.Move.WasPressedThisFrame() && _isAiming == false)
            {
                _dragStartPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
                _isAiming = true;
                StartSlowMotion();
            }
            
            if (_input.Player.Move.IsPressed() && _isAiming) // Draw the Aim Line
            {
                Vector3 screenPos = _input.Player.PointerPosition.ReadValue<Vector2>();
                screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
                Vector2 currentTouchPos = _mainCamera.ScreenToWorldPoint(screenPos);

                Vector2 bisector = (currentTouchPos - _dragStartPosition);
                List<Vector2> directions = GetSplitShapeDirections(bisector, currentTouchPos);
                _trajectorySimulator.DrawTrajectory(transform.position, directions, shootPower);
            }

            if (_input.Player.Move.WasReleasedThisFrame() && _isAiming)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0.0f;
                _trajectorySimulator.ClearLinePositions();
                _dragEndPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
                Vector2 dragDirection = _dragEndPosition - _dragStartPosition; //Bisector

                _isAiming = false;
                SplitPlayerAndApplyForce(dragDirection, _dragStartPosition, _dragEndPosition);
                EndSlowMotion();
            }
        }
        else if (_playerState == PlayerState.SPLIT)
        {
            if (_input.Player.Move.IsPressed())
            {
                UnitePlayerParts();
                SetSplitShapesColliderToTrigger(true);
            }
            else
            {
                SetSplitShapesColliderToTrigger(false);
            }

        }
    }


    private List<Vector2> GetSplitShapeDirections(Vector2 bisector, Vector2 currentTouchPosition)
    {
        float GetHalfAngle()
        {
            float dragDistance = Vector2.Distance(_dragStartPosition, currentTouchPosition);
            float t = Mathf.Clamp01(dragDistance / _maxDragDistance);
            float halfAngle = Mathf.Lerp(_minSpreadAngle * 0.5f, _maxSpreadAngle * 0.5f, t);
            return halfAngle;
        }

        List<Vector2> result = new List<Vector2>(_splitShapeCount);
        float halfAngle = GetHalfAngle();

        float startAngle = -halfAngle;
        float step = (halfAngle * 2) / (_splitShapeCount - 1);

        for (int i = 0; i < result.Capacity; i++)
        {
            float angle = startAngle + step * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * bisector.normalized;
            result.Add(direction);
        }

        return result;
    }

    private void StartSlowMotion()
    {
        Time.timeScale = 0.5f;
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
