using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        public SoundMixerManager soundMixerManager;
        public UIDocument uiDocument;

        private Button _playButton;
        private Button _optionsButton;
        private Button _exitButton;
    
        private Slider _masterVolSlider;
        private Slider _soundFXVolSlider;
        private Slider _musicVolSlider;
    
        private OptionsMenuManager _optionsMenuManager;
        private VisualElement _optionsMenu;
        private VisualElement _mainMenu;

        private Button _backToMainMenuButton;
        private Button _audioOptionsButton;
        private Button _graphicsOptionsButton;
    
        void Start()
        {
            VisualElement root = uiDocument.rootVisualElement;

            _optionsMenuManager = new OptionsMenuManager();
            _optionsMenuManager.Initialize(uiDocument);
            
            _optionsMenu = root.Q<VisualElement>("OptionsMenu");
            _mainMenu = root.Q<VisualElement>("MainMenu");
        
            _backToMainMenuButton =  root.Q<Button>("BackButton");
            _playButton = root.Q<Button>("PlayButton");
            _optionsButton = root.Q<Button>("OptionsButton");
            _exitButton = root.Q<Button>("ExitButton");
            _audioOptionsButton = root.Q<Button>("AudioButton");
            _graphicsOptionsButton = root.Q<Button>("GraphicsButton");
            
            
            _masterVolSlider = root.Q<Slider>("MasterVol");
            _soundFXVolSlider = root.Q<Slider>("SoundFXVol");
            _musicVolSlider = root.Q<Slider>("MusicVol");
        
            _masterVolSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
            _soundFXVolSlider.RegisterValueChangedCallback(OnSoundFXVolumeChanged);
            _musicVolSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
        
            _playButton.clicked += OnPlayButtonPressed;
            _optionsButton.clicked += OnOptionsButtonPressed;
            _exitButton.clicked += OnExitButtonPressed;
            _backToMainMenuButton.clicked += OnBackToMainMenuButtonPressed;
            _audioOptionsButton.clicked += OnAudioOptionsButtonPressed;
            _graphicsOptionsButton.clicked += OnGraphicsOptionsButtonPressed;
            
            _mainMenu.style.display = DisplayStyle.Flex;
            _optionsMenu.style.display = DisplayStyle.None;
        }


        private void OnPlayButtonPressed()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
        private void OnOptionsButtonPressed()
        {
            _mainMenu.style.display = DisplayStyle.None;
            AnimateAndShowMenu(_optionsMenu);
        }
        private void OnExitButtonPressed()
        {
            Application.Quit();
        }

        //////////// OPTIONS MENU ////////////
        //// AUDIO ////
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
        //// GRAPHICS ////
        
        
        
        private void OnAudioOptionsButtonPressed()
        {
            _optionsMenuManager.Show(MenuType.AudioMenu);
        }
        private void OnGraphicsOptionsButtonPressed()
        {
            _optionsMenuManager.Show(MenuType.GraphicsMenu);
        }
        private void OnBackToMainMenuButtonPressed()
        {
            _optionsMenu.style.display = DisplayStyle.None;
            AnimateAndShowMenu(_mainMenu);
        }

        private void AnimateAndShowMenu(VisualElement menuToShow)
        {
            menuToShow.style.display = DisplayStyle.Flex;
            menuToShow.style.opacity = 0f;
            menuToShow.style.scale = Vector3.one * 0.95f;

            menuToShow.experimental.animation
                .Start(0f, 1f, 369, (VisualElement m, float value) =>
                {
                    menuToShow.style.opacity = value;
                    menuToShow.style.scale = Vector3.one * Mathf.Lerp(0.95f, 1f, value);
                });
        }
    }
}
