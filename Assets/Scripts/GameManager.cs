using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool _isPlayerAlive = true;
    private bool _isPaused = false;

    public event Action<bool> OnPauseChanged;
    public event Action OnPlayerDied;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
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

        if (!_isPlayerAlive)
            OnPlayerDied?.Invoke();
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

