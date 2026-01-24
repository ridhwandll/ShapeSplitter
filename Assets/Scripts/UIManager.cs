using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    private Label _healthBar;
    
    private Button _backToMMBtn;
    
    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;
        _healthBar = root.Q<Label>("HealthBar");
        _backToMMBtn =  root.Q<Button>("BackToMMBtn");

        _backToMMBtn.clicked += OnBackToMMButtonPressed;
    }
        
    public void UpdatePlayerHealth(int playerHealth)
    {
        _healthBar.text = "Health: " + playerHealth;
    }

    private void OnBackToMMButtonPressed()
    {
        SceneManager.LoadScene(0); // Load main menu   
    }
}
