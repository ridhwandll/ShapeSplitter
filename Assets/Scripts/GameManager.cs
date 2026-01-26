using System;
using System.Collections;
using UnityEngine;
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
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (volume.profile.TryGet<Bloom>(out var b))
            b.active = Globals.Bloom;
        if (volume.profile.TryGet<Vignette>(out var v))
            v.active = Globals.Vignette;
        if (volume.profile.TryGet<Tonemapping>(out var t))
            t.active = Globals.Tonemapping;
        
        _isPaused = false;
        OnPauseChanged?.Invoke(_isPaused);
    }
    
    private void SetPaused(bool pause)
    {
        if (_isPaused == pause)
            return;

        _isPaused = pause;

        OnPauseChanged?.Invoke(_isPaused);
    }

    public void SetPlayerAlive(bool isAlive)
    {
        _isPlayerAlive = isAlive;

        if (!_isPlayerAlive) //Game over here
        {
            OnPlayerDied?.Invoke();
            SoundFXManager.instance.PlaySoundFXClip(gameOverClip, transform, 1.0f);
        }
    }
    
    public bool IsPaused => _isPaused;
    public bool IsPlayerAlive => _isPlayerAlive;
    
    void Update()
    {
        // Pause
        if (Input.GetKeyDown(KeyCode.Escape) && _isPlayerAlive)
        {
            if (_isPaused)
                SetPaused(false);
            else
                SetPaused(true);
        }
    }
}

