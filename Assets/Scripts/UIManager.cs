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

    [Header("Dash ChainShot and Repulsor bar")]
    public Slider dashSlider;
    public Image dashBar;
    public TMP_Text dashBarText;
    
    public Slider chainShotSlider;
    public Image chainShotBar;
    public TMP_Text chainShotBarText;
    
    public Slider repulsorSlider;
    public Image repulseBar;
    public TMP_Text repulseBarText;
    
    [Header("Tips")]
    public TMP_Text howToDashText;
    public TMP_Text howToChainShotText;
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
    }

    public void UpdateDashSlider(float timeLeftTillNextDash)
    {
        dashSlider.value = timeLeftTillNextDash;
        if (dashSlider.value >= Globals.PlayerDashCooldown)
        {
            dashBarText.text = "DASH [-" + Globals.DashSelfDamage + "]";
            dashBar.color = Color.gold;
            dashBarText.color = Color.gray1;
            if (Globals.DashTipShown == false)
            {
                StartCoroutine(ShowFadeInAndOut(howToDashText));
                Globals.DashTipShown = true;
            }
        }
        else
        {
            dashBarText.text = "DASH";
            dashBar.color = Color.gray1;
            dashBarText.color = Color.gray8;
        }
            
    }
    
    public void UpdateRepulsorSlider(float timeLeftTillNextRepulse)
    {
        repulsorSlider.value = timeLeftTillNextRepulse;
        if (repulsorSlider.value >= Globals.RepulsorCooldown)
        {
            repulseBarText.text = "REPULSE [-" + Globals.RepulsorSelfDamage + "]";
            repulseBar.color = Color.darkOrange;
            repulseBarText.color = Color.gray1;
            if (Globals.RepulseTipShown == false)
            {
                StartCoroutine(ShowFadeInAndOut(howToRepulseText));
                Globals.RepulseTipShown = true;
            }
        }
        else
        {
            repulseBarText.text = "REPULSE";
            repulseBar.color = Color.gray1;
            repulseBarText.color = Color.whiteSmoke;
        }
            
    }

    public void UpdateChainShotSlider(float timeLeftTillNextcs)
    {
        chainShotSlider.value = timeLeftTillNextcs;
        if (chainShotSlider.value >= Globals.ChainShotCooldown)
        {
            chainShotBarText.text = "CHAIN SHOT [-" + Globals.ChainShotSelfDamage + "]";
            chainShotBar.color = new Color(1f, 0.6f, 0.1f, 1f);
            chainShotBarText.color = Color.gray1;
            if (Globals.ChainShotTipShown == false)
            {
                StartCoroutine(ShowFadeInAndOut(howToChainShotText));
                Globals.ChainShotTipShown = true;
            }
        }
        else
        {
            chainShotBarText.text = "CHAIN SHOT";
            chainShotBar.color = Color.gray1;
            chainShotBarText.color = Color.whiteSmoke;
        }
    }

    public void UpdatePlayerHealth(int playerHealth) // NOT CALLED EVERY FRAME
    {
        repulsorSlider.minValue = 0;
        dashSlider.minValue = 0;
        chainShotSlider.minValue = 0;
        healthSlider.minValue = 0;
            
        repulsorSlider.maxValue = Globals.RepulsorCooldown;
        dashSlider.maxValue = Globals.PlayerDashCooldown;
        chainShotSlider.maxValue = Globals.ChainShotCooldown;
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
        
        int finalScore = enemySpawner.GetFinalScore();
        if (Globals.Difficulty == DifficultyLevel.Easy)
        {
            enemyKillScoreText.text = "";
            difficultyAwardedText.text = "Difficulty Award: EASY MODE";
            totalScoreText.text = "Total Score: 00";
            coinsEarnedText.text = "COINS: +100";
            Globals.Coins += 100;
        }
        else
        {
            enemyKillScoreText.text = "Kill Score: " + enemySpawner.GetEnemyKillScore();
            difficultyAwardedText.text = "Wave Award: " + enemySpawner.GetWaveScore(Globals.Difficulty) + " // " + enemySpawner.GetWaveNumber() + " waves in " + Globals.Difficulty.ToString().ToUpper();
            totalScoreText.text = "Total Score: " + finalScore;
            int coinsEarned = Globals.ScoreToCoinConv(finalScore);
            coinsEarnedText.text = "COINS: +" + coinsEarned;
            Globals.Coins += coinsEarned;
        
            if (Globals.Highscore < finalScore)
                Globals.Highscore = finalScore;
        }
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
        if (Globals.Difficulty == DifficultyLevel.Easy)
        {
            scoreText.text = "";
        }
        else
        {
            scoreText.text = "SCORE: " + score;
        }
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
        float maxAlpha = 0.7f;
        
        // FadeIN
        float t = 0f;
        float fadeInDuration = 2f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInDuration);
            text.color = c;
            yield return null;
        }

        // Ensure fully visible
        c.a = maxAlpha;
        text.color = c;

        // Stay visible
        yield return new WaitForSeconds(showDuration);

        // Fade OUT
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutDuration);
            text.color = c;
            yield return null;
        }

        // Ensure invisible & disable
        c.a = 0f;
        text.color = c;
        text.gameObject.SetActive(false);
    }
}
