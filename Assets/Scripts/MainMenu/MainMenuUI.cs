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
    
        // OPTIONS MENU
        private Slider _masterVolSlider;
        private Slider _soundFXVolSlider;
        private Slider _musicVolSlider;
    
        private OptionsMenuManager _optionsMenuManager;
        private VisualElement _optionsMenu;
        private VisualElement _mainMenu;

        private Button _backToMainMenuButton;
        private Button _audioOptionsButton;
        private Button _graphicsOptionsButton;
        private Button _difficultyOptionsButton;
        
        //Graphics Toggles
        private Toggle _bloomToggle;
        private Toggle _vignetteToggle;
        private Toggle _tonemappingToggle;
        
        // Difficulty
        private RadioButtonGroup _difficultyOptionsGroup;
        
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
            _difficultyOptionsButton = root.Q<Button>("DifficultyButton");
            
            _masterVolSlider = root.Q<Slider>("MasterVol");
            _soundFXVolSlider = root.Q<Slider>("SoundFXVol");
            _musicVolSlider = root.Q<Slider>("MusicVol");
        
            //Graphics
            _bloomToggle = root.Q<Toggle>("BloomToggle");
            _tonemappingToggle = root.Q<Toggle>("TonemappingToggle");
            _vignetteToggle = root.Q<Toggle>("VignetteToggle");
            _bloomToggle.RegisterValueChangedCallback(evt => { Constants.Bloom = evt.newValue; });
            _vignetteToggle.RegisterValueChangedCallback(evt => { Constants.Vignette = evt.newValue; });
            _tonemappingToggle.RegisterValueChangedCallback(evt => { Constants.Tonemapping = evt.newValue; });
            
            _bloomToggle.value =  Constants.Bloom;
            _vignetteToggle.value =  Constants.Vignette;
            _tonemappingToggle.value =  Constants.Tonemapping;
            
            // Difficulty
            _difficultyOptionsGroup =root.Q<RadioButtonGroup>("DifficultyRadioButtons");
            _difficultyOptionsGroup.RegisterValueChangedCallback((evt) =>
            {
                Debug.Log("Selected index: " + ((DifficultyLevel)evt.newValue).ToString());
                switch (evt.newValue)
                {
                    case 0:
                        Constants.Difficulty = DifficultyLevel.Easy;
                        break;
                    case 1:
                        Constants.Difficulty = DifficultyLevel.Medium;
                        break;
                    case 2:
                        Constants.Difficulty = DifficultyLevel.Hard;
                        break;
                    case 3:
                        Constants.Difficulty = DifficultyLevel.Impossible;
                        break;
                }
            });
            _difficultyOptionsGroup.value = (int)Constants.Difficulty;
            
            _masterVolSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
            _soundFXVolSlider.RegisterValueChangedCallback(OnSoundFXVolumeChanged);
            _musicVolSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);

            _masterVolSlider.value = Constants.MasterVolume;
            _soundFXVolSlider.value = Constants.SoundFXVolume;
            _musicVolSlider.value = Constants.MusicVolume;
            
            _playButton.clicked += OnPlayButtonPressed;
            _optionsButton.clicked += OnOptionsButtonPressed;
            _exitButton.clicked += OnExitButtonPressed;
            _backToMainMenuButton.clicked += OnBackToMainMenuButtonPressed;
            _audioOptionsButton.clicked += () => { _optionsMenuManager.Show(MenuType.AudioMenu); };
            _graphicsOptionsButton.clicked += () => { _optionsMenuManager.Show(MenuType.GraphicsMenu); };
            _difficultyOptionsButton.clicked += () => { _optionsMenuManager.Show(MenuType.DifficultyMenu); };
            
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
        
        
        //// DIFFICULTY ////
        
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
