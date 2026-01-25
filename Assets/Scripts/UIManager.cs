using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Health Bar
    public Slider healthSlider;
    public TMP_Text healthText;
    public TMP_Text waveNumber;
    public TMP_Text difficultyText;
    public Image healthBarBorder;
    
    public GameObject pauseMenu;
    public GameObject deathMenu;

    public EnemySpawner enemySpawner;
    
    // Dash bar
    public Slider dashSlider;
    
    private void Awake()
    {
        dashSlider.minValue = 0;
        dashSlider.maxValue = Constants.PlayerDashCooldown;

        healthSlider.minValue = 0;
        healthSlider.maxValue = Constants.PlayerMaxHealth;
    }

    void Start()
    {
        GameManager.Instance.OnPauseChanged += OnPauseChanged;
        GameManager.Instance.OnPlayerDied += OnPlayerDied;
        waveNumber.text = "WAVE: " + enemySpawner.GetDifficultyLevel();
        enemySpawner.OnDifficultyChanged += OnDifficultyChanged;
        difficultyText.text = Constants.Difficulty.ToString().ToUpper();
    }

    public void UpdateDashSlider(float timeLeftTillNextDash)
    {
        dashSlider.value = timeLeftTillNextDash;
    }

    public void UpdatePlayerHealth(int playerHealth) // NOT CALLED EVERY FRAME
    {
        healthSlider.value = playerHealth;
        healthText.text = playerHealth + "/" + Constants.PlayerMaxHealth;

        if (playerHealth <= Constants.PlayerMaxHealth / 3)
        {
            healthBarBorder.color = Color.softRed;
            healthText.color = Color.red;
        }
        else
        {
            healthBarBorder.color = Color.gray1;
            healthText.color = Color.white;
        }
    }

    private void OnPauseChanged(bool paused)
    {
        deathMenu.SetActive(false);
        pauseMenu.SetActive(paused);
    }

    private void OnPlayerDied()
    {
        pauseMenu.SetActive(false);
        deathMenu.SetActive(true);
    }

    private void OnDifficultyChanged(int difficulty)
    {
        waveNumber.text = "WAVE: " + difficulty;
    }
    
    public void OnReturnToMMPressed()
    {
        SceneManager.LoadSceneAsync(0); //Return to main menu
    }
}
