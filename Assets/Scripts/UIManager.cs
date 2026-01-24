using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public SoundMixerManager soundMixerManager;
    
    private Label _healthBar;

    private Slider _masterVolSlider;
    private Slider _soundFXVolSlider;
    private Slider _musicVolSlider;
    
    
    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;
        _healthBar = root.Q<Label>("HealthBar");
        _masterVolSlider = root.Q<Slider>("MasterVol");
        _soundFXVolSlider = root.Q<Slider>("SoundFXVol");
        _musicVolSlider = root.Q<Slider>("MusicVol");
        
        _masterVolSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
        _soundFXVolSlider.RegisterValueChangedCallback(OnSoundFXVolumeChanged);
        _musicVolSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
    }

    public void UpdatePlayerHealth(int playerHealth)
    {
        _healthBar.text = "Health: " + playerHealth;
    }

    private void OnMasterVolumeChanged(ChangeEvent<float> e)
    {
        soundMixerManager.SetMasterVolume(e.newValue);
    }
    private void OnSoundFXVolumeChanged(ChangeEvent<float> e)
    {
        soundMixerManager.SetSoundFXVolume(e.newValue);
    }
    private void OnMusicVolumeChanged(ChangeEvent<float> e)
    {
        soundMixerManager.SetMusicVolume(e.newValue);
    }
}
