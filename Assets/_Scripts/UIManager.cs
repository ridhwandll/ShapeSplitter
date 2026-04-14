using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Player")]
    public PlayerController player;

    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthBarBorder;
        
    [Header("Pause Menu")]
    public GameObject pauseMenu;

    [Header("Death Menu")]
    public GameObject deathMenu;
    public TMP_Text enemyKillScoreText;
    public TMP_Text difficultyAwardedText;
    public TMP_Text totalScoreText;
    public TMP_Text coinsEarnedText;
    
    public EnemySpawner enemySpawner;

    [Header("Tips")]
    public float showDuration = 7f;
    public float fadeOutDuration = 2f;

    public AudioClip buttonClickSound;
    
    void Start()
    {
        GameManager.Instance.OnPauseChanged += OnPauseChanged;
        GameManager.Instance.OnPlayerDied += OnPlayerDied;

        // Setup events which player controller notifies about
        player.OnPlayerHealthChange += health => UpdatePlayerHealth(health);
    }

    public void UpdatePlayerHealth(int playerHealth) // NOT CALLED EVERY FRAME
    {
        healthSlider.minValue = 0;
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
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.9f);
        
        deathMenu.SetActive(false);
        pauseMenu.SetActive(paused);
    }

    private void OnPlayerDied()
    {
        pauseMenu.SetActive(false);
        deathMenu.SetActive(true);
        PlayerProgress.Instance.Save();
    }

    private void OnEnemyDied()
    {       
    }
    
    private void OnWaveChanged(int difficulty)
    {
    }
    
    public void OnReturnToMMPressed()
    {
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.5f);
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
