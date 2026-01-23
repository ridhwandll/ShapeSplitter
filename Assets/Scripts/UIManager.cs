using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    private Label _healthBar;

    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;
        _healthBar = root.Q<Label>("HealthBar");
    }

    public void UpdatePlayerHealth(int playerHealth)
    {
        _healthBar.text = "Health: " + playerHealth;
    }
}
