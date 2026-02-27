using System;
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
    [SerializeField] private Shape _shapeData;
    [SerializeField] private float _maxDragDistance = 10.0f;
    [SerializeField] private GameObject _playerSplitPrefab;
    [SerializeField] private ParticleSystem playerParticleSystem;

    // AIM
    private Vector2 _dragStartPosition;
    private Vector2 _dragEndPosition;
    private bool _isAiming;
    
    private Rigidbody2D[] _splitObjectsRigidbody2D;
    private bool[] _isSplitShapeUnited;

    private PlayerState _playerState;

    // Health
    private int _health;

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
            new GradientColorKey[] { new GradientColorKey(_shapeData.ShapeThemeColorOne, 0.0f), new GradientColorKey(_shapeData.ShapeThemeColorTwo, 1.0f)},
            new GradientAlphaKey[] {new GradientAlphaKey(1.0f, 0.0f),new GradientAlphaKey(0.3f, 1.0f)});

        _splitObjectsRigidbody2D = new Rigidbody2D[_shapeData.SplitShapeCount];
        _isSplitShapeUnited = new bool[_shapeData.SplitShapeCount];
        for (int j = 0; j < _shapeData.SplitShapeCount; j++)
        {
            _isSplitShapeUnited[j] = false;
            _splitObjectsRigidbody2D[j] = Instantiate(_playerSplitPrefab, transform).GetComponent<Rigidbody2D>();
            _splitObjectsRigidbody2D[j].GetComponent<SpriteRenderer>().sprite = _shapeData.PlayerSplit;
            _splitObjectsRigidbody2D[j].GetComponent<TrailRenderer>().colorGradient = gradient;
            _splitObjectsRigidbody2D[j].gameObject.SetActive(false);
        }

        _spriteRenderer.sprite = _shapeData.PlayerUnited;
        _trailRenderer.colorGradient = gradient;

        // Do not simulate ricochet by default
        _trajectorySimulator.Initialize(_shapeData.SplitShapeCount, _shapeData.ShapeThemeColorOne, _shapeData.ShapeThemeColorTwo);
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
        _spriteRenderer.sprite = _shapeData.PlayerCore;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _trailRenderer.emitting = false;

        var directions = GetSplitShapeDirections(biesctorDirection, dragEndPoint);

        int i = 0;
        foreach (Rigidbody2D splitObjectRb in _splitObjectsRigidbody2D)
        {
            splitObjectRb.gameObject.SetActive(true);
            splitObjectRb.AddForce(directions[i].normalized * _shapeData.ShootPower, ForceMode2D.Impulse);
            i++;
        }

        // Add force to the core
        _rigidbody2D.GetComponent<Rigidbody2D>().AddForce(biesctorDirection.normalized * (_shapeData.ShootPower / 4), ForceMode2D.Impulse);
    }

    
    private void UnitePlayerParts()
    {
        for (int i = 0; i < _shapeData.SplitShapeCount; i++)
        {
            Rigidbody2D splitObjectRb = _splitObjectsRigidbody2D[i];

            splitObjectRb.linearVelocity = (_rigidbody2D.position - (Vector2)splitObjectRb.transform.position).normalized * _shapeData.ShootPower;
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
            _spriteRenderer.sprite = _shapeData.PlayerUnited;
            _trailRenderer.emitting = true;

            for (int j = 0; j < _isSplitShapeUnited.Length; j++)
                _isSplitShapeUnited[j] = false;
        }
    }

    private void AimAndShoot()
    {
        void SetSplitShapesColliderToTrigger(bool trigger)
        {
            for (int i = 0; i < _shapeData.SplitShapeCount; i++)
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

                float angle = Mathf.Atan2(bisector.y, bisector.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                _trajectorySimulator.DrawTrajectories(transform.position, directions, _shapeData.ShootPower);
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
            playerParticleSystem.Play();

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
