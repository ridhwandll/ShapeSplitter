using UnityEngine;

namespace MainMenu
{
    public class AudioDontDestroyOnLoad : MonoBehaviour
    {
        public static AudioDontDestroyOnLoad Instance;

        void Start()
        {
            // Singleton guard (prevents duplicates)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
