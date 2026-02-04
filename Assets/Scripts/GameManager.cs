using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Volume volume;
    public AudioClip gameOverClip;
    
    private bool _isPlayerAlive = true;
    private bool _isPaused = false;
    
    public event Action<bool> OnPauseChanged;
    public event Action OnPlayerDied;
    public event Action JustBeforePlayerDied;

    public InputMaster _input;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _input = new InputMaster();

        if (volume.profile.TryGet<Bloom>(out var b))
            b.active = Globals.Bloom;
        if (volume.profile.TryGet<Vignette>(out var v))
            v.active = Globals.Vignette;
        if (volume.profile.TryGet<Tonemapping>(out var t))
            t.active = Globals.Tonemapping;

        _input = InputManager.Instance.Input;
        _isPaused = false;
        OnPauseChanged?.Invoke(_isPaused);
    }
    

    public void InvalidatePaused()
    {
        if (_isPaused == false)
        {
            _isPaused = true;
        }
        else
        {
            _isPaused = false;
        }

        OnPauseChanged?.Invoke(_isPaused);
    }

    public void SetPlayerAlive(bool isAlive)
    {
        _isPlayerAlive = isAlive;

        if (!_isPlayerAlive) //Game over here
        {
            JustBeforePlayerDied?.Invoke();
            OnPlayerDied?.Invoke();
            SoundFXManager.instance.PlaySoundFXClip(gameOverClip, 1.0f);
        }
    }
    
    public bool IsPaused => _isPaused;
    public bool IsPlayerAlive => _isPlayerAlive;
    
    void Update()
    {
        // Pause
        if (_input.Game.Pause.WasPressedThisFrame() && _isPlayerAlive)
        {
            InvalidatePaused();
        }
    }
}

