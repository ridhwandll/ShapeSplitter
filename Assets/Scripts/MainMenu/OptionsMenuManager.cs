using UnityEngine;
using UnityEngine.UIElements;

namespace MainMenu
{
    public enum MenuType
    {
        AudioMenu,
        GraphicsMenu,
        DifficultyMenu,
    }

    public class OptionsMenuManager
    {
        private VisualElement _audioMenu;
        private VisualElement _graphicsMenu;
        private VisualElement _difficultyMenu;

        public void Initialize(UIDocument document)
        {
            _audioMenu = document.rootVisualElement.Q<VisualElement>("AudioMenu");
            _graphicsMenu = document.rootVisualElement.Q<VisualElement>("GraphicsMenu");
            _difficultyMenu = document.rootVisualElement.Q<VisualElement>("DifficultyMenu");
        }
    
        public void Show(MenuType menuType)
        {
            VisualElement menu = GetMenu(menuType);

            // Hide All
            _audioMenu.style.display = DisplayStyle.None;
            _graphicsMenu.style.display = DisplayStyle.None;
            _difficultyMenu.style.display = DisplayStyle.None;

            menu.style.display = DisplayStyle.Flex;
            menu.style.opacity = 0f;
            menu.style.scale = Vector3.one * 0.95f;

            menu.experimental.animation
                .Start(0f, 1f, 369, (VisualElement m, float value) =>
                {
                    menu.style.opacity = value;
                    menu.style.scale = Vector3.one * Mathf.Lerp(0.95f, 1f, value);
                });
        }
    
        private VisualElement GetMenu(MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.AudioMenu:
                    return _audioMenu;
                case MenuType.GraphicsMenu:
                    return _graphicsMenu;
                case MenuType.DifficultyMenu:
                    return _difficultyMenu;
            }
            return null;
        }
    }
}