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
    public Image dashBar;
    public Slider repulsorSlider;
    public Image repulseBar;
    
    [Header("Tips")]
    public TMP_Text howToDashText;
    public TMP_Text howToRepulseText;
    public TMP_Text howLifeShardWorksText;
    public float showDuration = 7f;
    public float fadeOutDuration = 2f;
    
    void Start()
    {
        GameManager.Instance.OnPauseChanged += OnPauseChanged;
        GameManager.Instance.OnPlayerDied += OnPlayerDied;
        waveNumber.text = "WAVE: " + enemySpawner.GetWaveNumber();
        enemySpawner.OnDifficultyChanged += OnDifficultyChanged;
        enemySpawner.OnEnemyKilled += OnEnemyDied;
        difficultyText.text = Globals.Difficulty.ToString().ToUpper();
        scoreText.text = "SCORE: " + enemySpawner.GetEnemyKillScore();
        coinsText.text = "COINS: 0";
    }

    public void UpdateDashSlider(float timeLeftTillNextDash)
    {
        dashSlider.value = timeLeftTillNextDash;
        if (dashSlider.value >= Globals.PlayerDashCooldown)
        {
            dashBar.color = Color.gold;
            if (Globals.DashTipShown == false)
            {
                StartCoroutine(ShowFadeInAndOut(howToDashText));
                Globals.DashTipShown = true;
            }
        }
        else
            dashBar.color = Color.gray1;
            
    }
    
    public void UpdateRepulsorSlider(float timeLeftTillNextRepulse)
    {
        repulsorSlider.value = timeLeftTillNextRepulse;
        if (repulsorSlider.value >= Globals.RepulsorCooldown)
        {
            repulseBar.color = Color.darkOrange;
            if (Globals.RepulseTipShown == false)
            {
                StartCoroutine(ShowFadeInAndOut(howToRepulseText));
                Globals.RepulseTipShown = true;
            }
        }
        else
            repulseBar.color = Color.gray1;
            
    }

    public void UpdatePlayerHealth(int playerHealth) // NOT CALLED EVERY FRAME
    {
        repulsorSlider.maxValue = Globals.RepulsorCooldown;
        dashSlider.maxValue = Globals.PlayerDashCooldown;
        healthSlider.maxValue = Globals.PlayerMaxHealth;
        
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
        if (Globals.LifeShardTipShown == false)
        {
            StartCoroutine(ShowFadeInAndOut(howLifeShardWorksText));
            Globals.LifeShardTipShown = true;
        }
        
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
        GameObject.FindGameObjectWithTag("LevelTransition").GetComponent<LevelTransition>().LoadMainMenu();
    }
    
    
    
    IEnumerator ShowFadeInAndOut(TMP_Text text)
    {
        text.gameObject.SetActive(true);

        Color c = text.color;
        c.a = 0f;
        text.color = c;
        
        // FadeIN
        float t = 0f;
        float fadeInDuration = 2f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            text.color = c;
            yield return null;
        }

        // Ensure fully visible
        c.a = 0.7f;
        text.color = c;

        // Stay visible
        yield return new WaitForSeconds(showDuration);

        // Fade OUT
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            text.color = c;
            yield return null;
        }

        // Ensure invisible & disable
        c.a = 0f;
        text.color = c;
        text.gameObject.SetActive(false);
    }
}
