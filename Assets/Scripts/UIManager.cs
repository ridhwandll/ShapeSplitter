using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Health Bar
    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthBarBorder;
    
    [Header("LevelInfo")]
    public TMP_Text waveNumber;
    public TMP_Text difficultyText;
    public TMP_Text scoreText;
    public TMP_Text coinsText;
    
    [Header("Pause Menu")]
    public GameObject pauseMenu;

    //Death menu stuff
    [Header("Death Menu")]
    public GameObject deathMenu;
    public TMP_Text enemyKillScoreText;
    public TMP_Text difficultyAwardedText;
    public TMP_Text totalScoreText;
    public TMP_Text coinsEarnedText;
    
    public EnemySpawner enemySpawner;

    // Dash bar
    public Slider dashSlider;
    
    private void Awake()
    {
        dashSlider.minValue = 0;
        dashSlider.maxValue = Globals.PlayerDashCooldown;

        healthSlider.minValue = 0;
        healthSlider.maxValue = Globals.PlayerMaxHealth;
    }

    void Start()
    {
        GameManager.Instance.OnPauseChanged += OnPauseChanged;
        GameManager.Instance.OnPlayerDied += OnPlayerDied;
        waveNumber.text = "WAVE: " + enemySpawner.GetWaveNumber();
        enemySpawner.OnDifficultyChanged += OnDifficultyChanged;
        enemySpawner.OnEnemyKilled += OnEnemyDied;
        difficultyText.text = Globals.Difficulty.ToString().ToUpper();
        scoreText.text = "SCORE: " + enemySpawner.GetEnemyKillScore();
        coinsText.text = "COINS: 00";
    }

    public void UpdateDashSlider(float timeLeftTillNextDash)
    {
        dashSlider.value = timeLeftTillNextDash;
    }

    public void UpdatePlayerHealth(int playerHealth) // NOT CALLED EVERY FRAME
    {
        healthSlider.value = playerHealth;
        healthText.text = playerHealth + "/" + Globals.PlayerMaxHealth;

        if (playerHealth <= Globals.PlayerMaxHealth / 3)
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
        
        enemyKillScoreText.text = "Kill Score: " + enemySpawner.GetEnemyKillScore();
        difficultyAwardedText.text = "Difficulty Award: " + enemySpawner.GetDifficultyScore(Globals.Difficulty);

        int finalScore = enemySpawner.GetFinalScore();
        totalScoreText.text = "Total Score: " + finalScore;

        int coinsEarned = Globals.ScoreToCoinConv(finalScore);
        coinsEarnedText.text = "COINS: +" + coinsEarned;
        Globals.Coins += coinsEarned;
        
        if (Globals.Highscore < finalScore)
            Globals.Highscore = finalScore;
        
        PlayerProgress.Instance.Save();
    }

    private void OnEnemyDied()
    {
        var score = enemySpawner.GetEnemyKillScore();
        scoreText.text = "SCORE: " + score;
        coinsText.text = "COINS: " + Globals.ScoreToCoinConv(score);
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
