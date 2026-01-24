using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private bool _paused = false;
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthBarBorder;
    public GameObject pauseMenu;
    
    private void Awake()
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = Constants.PlayerMaxHealth;
    }
        
    public void UpdatePlayerHealth(int playerHealth)
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
            healthText.color = Color.gray2;
        }
            
    }

    private void Update()
    {
        //TODO: Implement Real Escape Here
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused)
            {
                _paused = false;
                pauseMenu.SetActive(false);
            }
            else
            {
                pauseMenu.SetActive(true);
                _paused = true;
                
            }
        }
    }

    public void OnReturnToMMPressed()
    {
        SceneManager.LoadSceneAsync(0);
    }
    
    private void OnBackToMMButtonPressed()
    {
        SceneManager.LoadScene(0); // Load main menu   
    }
}
