using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
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
    [SerializeField] private Shape _shapeData;
    [SerializeField] private float _maxDragDistance = 5.0f;
    [SerializeField] private float _minDragDistance = 1.0f;
    [SerializeField] private GameObject _playerSplitPrefab;
    [SerializeField] private ParticleSystem _splitShapeReturnPS;

    // AIM
    private Vector2 _dragStartPosition;
    private Vector2 _dragEndPosition;
    private bool _isAiming;
    private bool _isHolding;
    private Rigidbody2D[] _splitObjectsRigidbody2D;
    private bool[] _isSplitShapeUnited;

    private PlayerState _playerState;

    // Particle system
    ParticleSystem _splitShapeReturnParticleSystem;

    // Health
    private int _health = 150;

    private Rigidbody2D _rigidbody2D;
    private TrajectorySimulator _trajectorySimulator;
    private SpriteRenderer _spriteRenderer;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private Camera _mainCamera;
    private TrailRenderer _trailRenderer;

    private InputMaster _input;

    // Public Events available for registering (Mainly used by UIManager)
    public Action<int> OnPlayerHealthChange;

    public Shape GetShapeData()
    {
        return _shapeData;
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        GameObject postProcessStack = GameObject.FindGameObjectWithTag("PostProcessStack");
        postProcessStack.GetComponent<Volume>().profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        postProcessStack.GetComponent<Volume>().profile.TryGet<Vignette>(out _vignette);
        _trailRenderer = GetComponent<TrailRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trajectorySimulator = GetComponent<TrajectorySimulator>();

        _chromaticAberration.active = false;
        _input = GameObject.FindGameObjectWithTag("InputMaster").GetComponent<InputManager>().Input;
        _playerState = PlayerState.UNITED;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(_shapeData.ShapeThemeColorOne, 0.0f), new GradientColorKey(_shapeData.ShapeThemeColorTwo, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.3f, 1.0f) });

        _splitObjectsRigidbody2D = new Rigidbody2D[_shapeData.SplitShapeCount];
        _isSplitShapeUnited = new bool[_shapeData.SplitShapeCount];
        for (int j = 0; j < _shapeData.SplitShapeCount; j++)
        {
            _isSplitShapeUnited[j] = false;
            _splitObjectsRigidbody2D[j] = Instantiate(_playerSplitPrefab, transform).GetComponent<Rigidbody2D>();
            _splitObjectsRigidbody2D[j].GetComponent<SpriteRenderer>().sprite = _shapeData.ShapeSplitSprite;
            _splitObjectsRigidbody2D[j].GetComponent<TrailRenderer>().colorGradient = gradient;
            RecalculatePolygonCollider(_splitObjectsRigidbody2D[j].gameObject);
            _splitObjectsRigidbody2D[j].gameObject.SetActive(false);
        }

        _spriteRenderer.sprite = _shapeData.ShapeUnitedSprite;
        _trailRenderer.colorGradient = gradient;
        RecalculatePolygonCollider(gameObject);

        _trajectorySimulator.Initialize(_shapeData.SplitShapeCount, _shapeData.ShapeThemeColorOne, _shapeData.ShapeThemeColorTwo);

        _input.Player.Ability1.performed += ctx => { _shapeData.Ability.TryActivate(this); };
        _input.Player.Ultimate.performed += ctx => { _shapeData.Ultimate.TryActivate(this); };

        // Set the PSs 
        {
            _splitShapeReturnParticleSystem = Instantiate(_splitShapeReturnPS, transform);
            ParticleSystem.MainModule main = _splitShapeReturnParticleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(_shapeData.ShapeThemeColorOne, _shapeData.ShapeThemeColorTwo);
        }
    }

    private void OnDestroy()
    {
        if (_shapeData.Ability) _shapeData.Ability.TryDeactivate(this);
        if (_shapeData.Ultimate) _shapeData.Ultimate.TryDeactivate(this);
    }

    private void RecalculatePolygonCollider(GameObject go)
    {
        PolygonCollider2D existing = go.GetComponent<PolygonCollider2D>();
        if (existing != null)
            Destroy(existing);

        PolygonCollider2D poly = go.AddComponent<PolygonCollider2D>();
        // Automatically generates points from SpriteRenderer
    }

    void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        AimAndShoot();
    }

    private void SplitPlayerAndApplyForce(Vector2 biesctorDirection, Vector2 dragStartPoint, Vector2 dragEndPoint)
    {
        _playerState = PlayerState.SPLIT;
        _spriteRenderer.sprite = _shapeData.ShapeCoreSprite;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _trailRenderer.emitting = false;

        var directions = GetSplitShapeDirections(biesctorDirection, dragEndPoint);

        int i = 0;
        foreach (Rigidbody2D splitObjectRb in _splitObjectsRigidbody2D)
        {
            splitObjectRb.gameObject.SetActive(true);            
            splitObjectRb.AddForce(directions[i].normalized * _shapeData.SplitShapeShootSpeed, ForceMode2D.Impulse);
            i++;
        }

        // Add force to the core
        _rigidbody2D.AddForce(biesctorDirection.normalized * _shapeData.CoreMoveSpeed, ForceMode2D.Impulse);
    }

    private void CheckIfAllSplitPartsAreUnited()
    {
        if (_isSplitShapeUnited.All(n => n == true))
        {
            _playerState = PlayerState.UNITED;
            _spriteRenderer.sprite = _shapeData.ShapeUnitedSprite;
            _trailRenderer.emitting = true;

            for (int j = 0; j < _isSplitShapeUnited.Length; j++)
            {
                _isSplitShapeUnited[j] = false;
                _splitObjectsRigidbody2D[j].gameObject.GetComponent<PlayerSplitShape>().ResetCollider(); // Resets the collider to not trigger
            }

            _rigidbody2D.angularVelocity = 0.0f;
            transform.rotation = Quaternion.Euler(0f, 0f, 0.0f);
        }
    }

    private void UnitePlayerParts_Anchor()
    {
        for (int i = 0; i < _shapeData.SplitShapeCount; i++)
        {
            Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];
            splitObjectRb.gameObject.GetComponent<Collider2D>().isTrigger = true;

            splitObjectRb.linearVelocity = (_rigidbody2D.position - (Vector2)splitObjectRb.transform.position).normalized * _shapeData.SplitShapeReturnSpeed;
            splitObjectRb.transform.localPosition = Vector3.Lerp(splitObjectRb.transform.localPosition, Vector3.zero, Time.unscaledDeltaTime * 7);
            splitObjectRb.angularVelocity = 0.0f;

            if (splitObjectRb.transform.localPosition.magnitude < 1f)
            {
                if (splitObjectRb.gameObject.activeInHierarchy)
                {
                    _isSplitShapeUnited[i] = true;
                    splitObjectRb.transform.localPosition = Vector3.zero;
                    splitObjectRb.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    _splitShapeReturnParticleSystem.Play();
                    splitObjectRb.GetComponent<TrailRenderer>().Clear();
                    splitObjectRb.gameObject.SetActive(false);
                }
            }
        }
        CheckIfAllSplitPartsAreUnited();
    }

    private void UnitePlayerParts_Others()
    {
        for (int i = 0; i < _shapeData.SplitShapeCount; i++)
        {
            Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];

            if (splitObjectRb.gameObject.GetComponent<PlayerSplitShape>().CollidedWithOthersOnce)
            {
                splitObjectRb.gameObject.GetComponent<Collider2D>().isTrigger = true;
                Vector2 normalizedDSirectionToCore = (_rigidbody2D.position - (Vector2)splitObjectRb.transform.position).normalized;

                // Removes the tangential velocity that causes orbiting                
                splitObjectRb.linearVelocity = Vector2.zero;
                splitObjectRb.linearVelocity = Vector3.Project(splitObjectRb.linearVelocity, normalizedDSirectionToCore);                
                splitObjectRb.AddForce(normalizedDSirectionToCore * _shapeData.SplitShapeReturnSpeed, ForceMode2D.Impulse);

                splitObjectRb.angularVelocity = 0.0f;
                _rigidbody2D.angularVelocity = 0.0f;

                if (splitObjectRb.transform.localPosition.magnitude < 1f)
                {
                    if (splitObjectRb.gameObject.activeInHierarchy)
                    {
                        _isSplitShapeUnited[i] = true;
                        splitObjectRb.transform.localPosition = Vector3.zero;
                        splitObjectRb.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        _splitShapeReturnParticleSystem.Play();
                        splitObjectRb.GetComponent<TrailRenderer>().Clear();
                        splitObjectRb.gameObject.SetActive(false);
                    }
                }
            }
        }
        CheckIfAllSplitPartsAreUnited();
    }

    void SetSplitShapesColliderToTrigger(bool trigger)
    {
        for (int i = 0; i < _shapeData.SplitShapeCount; i++)
        {
            Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];
            if (splitObjectRb.gameObject.activeInHierarchy)
            {
                splitObjectRb.gameObject.GetComponent<Collider2D>().isTrigger = trigger;
            }
        }
    }

    private void AimAndShoot()
    {
        if (_playerState == PlayerState.UNITED)
        {
            if (_input.Player.Move.WasPressedThisFrame() && _isAiming == false)
            {
                _dragStartPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
                _isHolding = true;
            }

            if (_isHolding && !_isAiming)
            {
                Vector2 currentPos = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());

                float dragDistance = Vector2.Distance(_dragStartPosition, currentPos);

                if (dragDistance >= _minDragDistance)
                {
                    // Start aim
                    _isAiming = true;
                    StartSlowMotion();
                }
            }

            if (_input.Player.Move.IsPressed() && _isAiming) // Draw the Aim Line
            {
                Vector3 screenPos = _input.Player.PointerPosition.ReadValue<Vector2>();
                screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
                Vector2 currentTouchPos = _mainCamera.ScreenToWorldPoint(screenPos);

                Vector2 bisector = (currentTouchPos - _dragStartPosition);
                if (bisector.magnitude >= _minDragDistance)
                {
                    List<Vector2> directions = GetSplitShapeDirections(bisector, currentTouchPos);

                    float angle = Mathf.Atan2(bisector.y, bisector.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);

                    _trajectorySimulator.DrawTrajectories(transform.position, directions, _shapeData.SplitShapeShootSpeed);
                }
            }

            if (_input.Player.Move.WasReleasedThisFrame() && _isAiming)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0.0f;
                _trajectorySimulator.ClearLinePositions();
                _dragEndPosition = _mainCamera.ScreenToWorldPoint(_input.Player.PointerPosition.ReadValue<Vector2>());
                Vector2 dragDirection = _dragEndPosition - _dragStartPosition; // Bisector

                _isAiming = false;
                SplitPlayerAndApplyForce(dragDirection, _dragStartPosition, _dragEndPosition);
                EndSlowMotion();
            }

            if (_input.Player.Move.WasReleasedThisFrame() && !_isAiming)
            {
                _isHolding = false;
            }
        }
        else if (_playerState == PlayerState.SPLIT)
        {
            // ANCHORs has the ability to control when the shapes come back to core
            if (_shapeData.Role == ShapeRole.ANCHOR)
            {
                if (_input.Player.Move.IsPressed())
                {
                    UnitePlayerParts_Anchor();                    
                }
                else
                {
                    SetSplitShapesColliderToTrigger(false);
                }
            }
            // SPLINTERs come back to core after its hit with any collider
            else if (_shapeData.Role == ShapeRole.SPLINTER)
            {
                UnitePlayerParts_Others();
            }
            else if (_shapeData.Role == ShapeRole.SHELL)
            {
                // idk what mechanism to implement here
            }
        }
    }

    private List<Vector2> GetSplitShapeDirections(Vector2 bisector, Vector2 currentTouchPosition)
    {
        float GetHalfAngle()
        {
            float dragDistance = Vector2.Distance(_dragStartPosition, currentTouchPosition);
            float t = Mathf.Clamp01(dragDistance / _maxDragDistance);
            float halfAngle = Mathf.Lerp(_shapeData.MinSpreadAngle * 0.5f, _shapeData.MaxSpreadAngle * 0.5f, t);
            return halfAngle;
        }

        List<Vector2> result = new List<Vector2>(_shapeData.SplitShapeCount);
        float halfAngle = GetHalfAngle();

        float startAngle = -halfAngle;
        float step = (halfAngle * 2) / (_shapeData.SplitShapeCount - 1);

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
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    private void EndSlowMotion()
    {
        Time.timeScale = 1.0f;
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
            _vignette.color.value = UnityEngine.Color.red;
        }
        
        if (_health == 0) //LEVEL END HERE
        {
            Destroy(gameObject);
            GameManager.Instance.SetPlayerAlive(false);
        }

        if (!isDamagingByOwnBullet)
        {
            //Play PS
        }

        OnPlayerHealthChange?.Invoke(_health);
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(Globals.PlayerMaxHealth, _health + amount);

        if (_health > 15 && _chromaticAberration)
        {
            _chromaticAberration.active = false;
            _vignette.color.value = UnityEngine.Color.black;
        }

        OnPlayerHealthChange?.Invoke(_health);
    }
}
