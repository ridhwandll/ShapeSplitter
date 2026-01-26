using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerProgressData
{
    public int coins;
    public int highscore;
}

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "ShapeSplitter_Progress.json");
        
    void Awake()
    {
        // Singleton guard (prevents duplicates)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
            
        Instance = this;
        Load();
        DontDestroyOnLoad(gameObject);
    }

    public void Save()
    {
        PlayerProgressData save = new PlayerProgressData();
        
        save.coins = Globals.Coins;
        save.highscore = Globals.Highscore;
        
        string json = JsonUtility.ToJson(save, true);
        string encrypted = SaveCrypto.Encrypt(json);
        File.WriteAllText(SavePath, encrypted);

        Debug.Log("Saved Progress to: " + SavePath);
    }
    
    public bool Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return false;
        }
        
        string encrypted = File.ReadAllText(SavePath);
        string json = SaveCrypto.Decrypt(encrypted);
        PlayerProgressData save =  JsonUtility.FromJson<PlayerProgressData>(json);

        Globals.Coins = save.coins;
        Globals.Highscore = save.highscore;
        
        Debug.Log("Loaded Progress");
        return true;
    }
}
